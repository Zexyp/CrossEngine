using System;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using CrossEngine.Platform.Windows;
using CrossEngine.Events;
using CrossEngine.Utils;
using CrossEngine.Display;
using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Logging;

namespace CrossEngine
{
    using ThreadingThreadState = System.Threading.ThreadState;

    public class RenderThread
    {
        Thread _thread;
        EventWaitHandle _waitHandle;
        EventWaitHandle _joinHandle;

        public Window Window { get; private set; } = null;

        bool _shouldStop = false;

        Queue<Event> _events = new Queue<Event>();

        public RendererAPI rapi;

        ConcurrentQueue<SceneRenderData> _renderData = new ConcurrentQueue<SceneRenderData>();

        public RenderThread()
        {
            _thread = new Thread(Loop);

            _waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            _joinHandle = new EventWaitHandle(true, EventResetMode.ManualReset);

            Window = new GLFWWindow();
        }

        public void Run()
        {
            _waitHandle.Set();
        }

        public void Start()
        {
            if (_thread.ThreadState != ThreadingThreadState.Unstarted) throw new Exception("Thread is already running.");

            _joinHandle.Reset();
            _thread.Start();
        }

        public void Join()
        {
            _joinHandle.WaitOne();
        }

        public void Stop()
        {
            if (_thread.ThreadState == ThreadingThreadState.Unstarted) throw new Exception("Thread wasn't started.");

            Join();

            _shouldStop = true;
            Run();
        }

        public Event? DequeueEvent() => _events.TryDequeue(out Event e) ? e : null;

        private unsafe void Loop()
        {
            Profiler.BeginScope($"{nameof(RenderThread)}.{nameof(RenderThread.Init)}");
            Init();
            Profiler.EndScope();

            rapi.SetClearColor(new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1.0f));

            do
            {
                _joinHandle.Set();
                _waitHandle.WaitOne();
                _joinHandle.Reset();

                Profiler.BeginScope("Render loop");

                Window.PollWindowEvents();
                Window.UpdateWindow();

                rapi.Clear();

                while (ThreadManager.RenderThreadActionQueue.TryDequeue(out Action action)) action.Invoke();
                // draw
                while (_renderData.TryDequeue(out SceneRenderData sceneData))
                {
                    for (int layerIndex = 0; layerIndex < sceneData.Layers.Count; layerIndex++)
                    {
                        SceneLayerRenderData layerData = sceneData.Layers[layerIndex];
                        foreach ((IRenderable Renderable, IList Objects) item in layerData.Data)
                        {
                            var rndbl = item.Renderable;
                            var objs = item.Objects;
                            rndbl.Begin(layerData.ProjectionViewMatrix);
                            for (int objectIndex = 0; objectIndex < objs.Count; objectIndex++)
                            {
                                rndbl.Submit((IObjectRenderData)objs[objectIndex]);
                            }
                            rndbl.End();
                        }
                    }
                }

                Profiler.EndScope();
            } while (!_shouldStop);

            Profiler.BeginScope($"{nameof(RenderThread)}.{nameof(RenderThread.Destroy)}");
            Destroy();
            Profiler.EndScope();

            _joinHandle.Set();
        }

        public void SubmitData(SceneRenderData data)
        {
            Debug.Assert(data != null);

            Profiler.Function();

            _renderData.Enqueue(data);
        }

        private void OnWindowEvent(Event e)
        {
            Debug.Assert(e != null);

            if (e is WindowResizeEvent)
            {
                var wre = e as WindowResizeEvent;
                rapi.SetViewport(0, 0, wre.Width, wre.Height);
            }
            _events.Enqueue(e);
        }

        private void Init()
        {
            Window.SetEventCallback(OnWindowEvent);
            Window.CreateWindow();

            rapi = RendererAPI.Create();
            rapi.Init();
        }

        private void Destroy()
        {
            Window.DestroyWindow();
            Window = null;
        }
    }
}
