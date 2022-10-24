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
using CrossEngine.Logging;

namespace CrossEngine
{
    using ThreadingThreadState = System.Threading.ThreadState;

    class RenderThread
    {
        public Window Window { get; private set; } = null;

        private readonly Thread _thread;
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly EventWaitHandle _runWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
        private ConcurrentQueue<Event> _events = new ConcurrentQueue<Event>();
        private RendererAPI _rapi;

        private bool _shouldStop = false;

        public RenderThread(RendererAPI rendererAPI)
        {
            _rapi = rendererAPI;

            _thread = new Thread(Loop);

            Window = new GLFWWindow();

            ThreadManager.SetRenderThread(_thread);
        }

        public void Start()
        {
            if (_thread.ThreadState != ThreadingThreadState.Unstarted)
                throw new InvalidOperationException("Thread is already running.");

            _waitHandle.Reset();
            _thread.Start();
        }

        public void Wait()
        {
            _waitHandle.WaitOne();
        }

        public void Begin()
        {
            _runWaitHandle.Set();
        }

        public void Stop()
        {
            if (_thread.ThreadState == ThreadingThreadState.Unstarted)
                throw new InvalidOperationException("Thread wasn't started.");

            Begin();

            _shouldStop = true;
        }

        public Event? DequeueEvent()
        {
            return _events.TryDequeue(out Event e) ? e : null;
        }

        private unsafe void Loop()
        {
            Profiler.BeginScope($"{nameof(RenderThread)}.{nameof(RenderThread.Init)}");
            Init();
            Profiler.EndScope();

            // handle is reset in the start method
            _waitHandle.Set();

            _runWaitHandle.Set();

            while (!_shouldStop)
            {
                _runWaitHandle.WaitOne();

                _waitHandle.Reset();
                
                _runWaitHandle.Set();
                if (_shouldStop)
                    continue;

                Profiler.BeginScope("Render loop");

                _rapi.Clear();

                while (ThreadManager.RenderThreadActionQueue.TryDequeue(out Action action))
                    action.Invoke();

                Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Render)}");
                Application.Instance.Render();
                Profiler.EndScope();

                Window.PollWindowEvents();
                Window.UpdateWindow();

                Profiler.EndScope();

                _waitHandle.Set();
            }

            // a way to wait until the windows closes
            _waitHandle.Reset();

            Profiler.BeginScope($"{nameof(RenderThread)}.{nameof(RenderThread.Destroy)}");
            Destroy();
            Profiler.EndScope();

            _waitHandle.Set();
        }

        private void OnWindowEvent(Event e)
        {
            Debug.Assert(e != null);
            if (e is WindowCloseEvent)
                Window.ShouldClose = false;

            if (e is WindowResizeEvent)
            {
                var wre = e as WindowResizeEvent;
                _rapi.SetViewport(0, 0, wre.Width, wre.Height);
            }
            _events.Enqueue(e);
        }

        private unsafe void Init()
        {
            ThreadManager.ConfigureCurrentThread();

            Window.SetEventCallback(OnWindowEvent);
            Window.CreateWindow();

            // set window icon
            var icon = Properties.Resources.DefaultWindowIcon.ToBitmap();
            ImageUtils.SwapChannels(icon, ImageUtils.ColorChannel.Red, ImageUtils.ColorChannel.Blue); // this will come and bite later :D
            System.Drawing.Imaging.BitmapData bitmapData = icon.LockBits(new System.Drawing.Rectangle(0, 0, icon.Width, icon.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, icon.PixelFormat);
            Window.SetIcon((void*)bitmapData.Scan0, (uint)bitmapData.Width, (uint)bitmapData.Height);
            icon.UnlockBits(bitmapData);
            icon.Dispose();

            _rapi.Init();
            _rapi.SetClearColor(new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1.0f));

        }

        private void Destroy()
        {
            Window.DestroyWindow();
            Window = null;
        }
    }
}
