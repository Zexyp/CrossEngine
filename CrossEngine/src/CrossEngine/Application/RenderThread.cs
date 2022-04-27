using System;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using CrossEngine.Platform.Windows;
using CrossEngine.Events;
using CrossEngine.Utils.Imaging;
using CrossEngine.Display;
using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Logging;
using CrossEngine.Layers;

namespace CrossEngine
{
    using ThreadingThreadState = System.Threading.ThreadState;

    public class EventThread
    {

    }

    public class RenderThread
    {
        Thread _thread;
        EventWaitHandle _waitHandle;
        EventWaitHandle _joinHandle;

        public Window Window { get; private set; } = null;

        bool _shouldStop = false;

        Queue<Event> _events = new Queue<Event>();

        public RendererAPI rapi;

        IReadOnlyCollection<Layer> _renderLayers = null;

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

                Window.PollWindowEvents();
                Window.UpdateWindow();

                rapi.Clear();

                while (ThreadManager.RenderThreadActionQueue.TryDequeue(out Action action)) action.Invoke();
                Application.Instance.Render();
            } while (!_shouldStop);

            Profiler.BeginScope($"{nameof(RenderThread)}.{nameof(RenderThread.Destroy)}");
            Destroy();
            Profiler.EndScope();

            _joinHandle.Set();
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

        private unsafe void Init()
        {
            Window.SetEventCallback(OnWindowEvent);
            Window.CreateWindow();

            var icon = Properties.Resources.DefaultWindowIcon.ToBitmap();
            ImageUtils.SwapChannels(icon, ImageUtils.ColorChannel.Red, ImageUtils.ColorChannel.Blue); // this will come and bite later :D
            System.Drawing.Imaging.BitmapData bitmapData = icon.LockBits(new System.Drawing.Rectangle(0, 0, icon.Width, icon.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, icon.PixelFormat);
            Window.SetIcon((void*)bitmapData.Scan0, (uint)bitmapData.Width, (uint)bitmapData.Height);
            icon.UnlockBits(bitmapData);
            icon.Dispose();

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
