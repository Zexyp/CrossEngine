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
        public bool Running { get; private set; } = false;

        private readonly Thread _thread;
        private readonly EventWaitHandle _waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly EventWaitHandle _runWaitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
        private RendererAPI _rapi;

        public event Action OnDestroy;
        public event Action OnInit;
        public event Action OnRender;
        public event Action<Event> OnEvent;

        private bool _shouldStop = false;

        public RenderThread(RendererAPI rendererAPI)
        {
            _rapi = rendererAPI;

            _thread = new Thread(Loop);

            Window = new GlfwWindow();

            ThreadManager.SetRenderThread(_thread);
        }

        public void Start()
        {
            if (_thread.ThreadState != ThreadingThreadState.Unstarted)
                throw new InvalidOperationException("Thread is already running.");

            _waitHandle.Reset();
            _thread.Start();

            Running = true;
        }

        public void Wait()
        {
            _waitHandle.WaitOne();
        }

        public void Begin()
        {
            _runWaitHandle.Set();
            _waitHandle.Reset();
        }

        public void Stop()
        {
            Running = false;

            _waitHandle.Reset();

            if (_thread.ThreadState == ThreadingThreadState.Unstarted)
                throw new InvalidOperationException("Thread wasn't started.");

            _shouldStop = true;
            Begin();
        }

        private unsafe void Loop()
        {
            Profiler.BeginScope($"{nameof(RenderThread)}.{nameof(RenderThread.Init)}");
            Init();
            Profiler.EndScope();

            _runWaitHandle.Set();

            while (!_shouldStop)
            {
                _waitHandle.Set();
                _runWaitHandle.WaitOne();
                _runWaitHandle.Reset();

                if (_shouldStop)
                    continue;

                Profiler.BeginScope("Render loop");

                _rapi.Clear();

                ThreadManager.ProcessRenderThread();

                OnRender?.Invoke();

                Window.PollWindowEvents();
                Window.UpdateWindow();

                Profiler.EndScope();
            }

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

            OnEvent?.Invoke(e);
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

            OnInit?.Invoke();
            ThreadManager.ProcessRenderThread();
        }

        private void Destroy()
        {
            ThreadManager.ProcessRenderThread();
            OnDestroy?.Invoke();

            Window.DestroyWindow();
            Window = null;
        }
    }
}
