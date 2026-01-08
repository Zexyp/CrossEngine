//#define LOG_FRAME_SKIP

using CrossEngine.Core.Services;
using CrossEngine.Events;
using CrossEngine.Platform;
using CrossEngine.Profiling;
using CrossEngine.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrossEngine.Logging;
using CrossEngine.Core;

namespace CrossEngine.Display
{
    // todo: consider isurface for window
    public class WindowService : Service, IScheduledService, IUpdatedService
    {
        public enum Mode
        {
            None = default,
            Sync,
            ThreadLoop,
        }

        public Window MainWindow { get; private set; }
        public event Action<Window, Event> WindowEvent;
        public event Action<Window> WindowUpdate;
        public int MaxFrameDuration = (int)(1d / 30 * 1000);

        Thread _windowThread;
        readonly SingleThreadedTaskScheduler _scheduler = new SingleThreadedTaskScheduler();
        Mode _mode;
        bool _running = false;
        AutoResetEvent _render = new AutoResetEvent(false);
        AutoResetEvent _main = new AutoResetEvent(false);
        private Logger _log = new Logger("window-service");

        public WindowService(Mode mode)
        {
            _mode = mode;
        }

        public override void OnInit()
        {
            _running = true;
            if (_mode == Mode.ThreadLoop)
            {
                _windowThread = new Thread(() => Application.ThreadWrapper(Loop)) { Name = "window" };
                _windowThread.Start();
            }
            else
                Setup();
        }

        public override void OnDestroy()
        {
            _running = false;
            if (_mode == Mode.ThreadLoop)
            {
                _render.WaitOne();
                _main.Set();
                _windowThread.Join();
            }
            else
                Destroy();
        }

        public Task Execute(Action action) => _scheduler.Schedule(action);
        public Task<TResult> Execute<TResult>(Func<TResult> func) => _scheduler.Schedule(func);

        public void Update()
        {
            _scheduler.RunOnCurrentThread();

            var w = MainWindow;
            WindowUpdate?.Invoke(w);
            w.Keyboard.Update();
            w.Mouse.Update();

            Profiler.BeginScope($"{nameof(CrossEngine.Display.Window)}.{nameof(CrossEngine.Display.Window.PollEvents)}");
            w.PollEvents();
            Profiler.EndScope();
        }

        private void Setup()
        {
            // setup
            MainWindow = PlatformHelper.CreateWindow();

            MainWindow.Event += OnWindowEvent;

            MainWindow.Create();

            _scheduler.RunOnCurrentThread();
        }

        private void Destroy()
        {
            _scheduler.RunOnCurrentThread();

            MainWindow.Event -= OnWindowEvent;

            // destroy
            MainWindow.Destroy();

            MainWindow.Dispose();
        }

        private void OnWindowEvent(Event e)
        {
            WindowEvent?.Invoke(MainWindow, e);

            Manager.SendEvent(e);
        }

        private void Loop()
        {
            Setup();

            while (_running)
            {
                if (_mode == Mode.ThreadLoop)
                {
                    Profiler.Function("signaling main");
                    _render.Set();
                    Profiler.Function("waiting for main");
                    _main.WaitOne();
                    _main.Reset();
                }

                Update();
            }

            _render.Set();

            Destroy();
        }

        public override void OnAttach()
        {
            
        }

        public override void OnDetach()
        {
            
        }

#if LOG_FRAME_SKIP
        bool _lastFrameSkipped = false;
#endif

        public void OnUpdate()
        {
            if (!_running)
                return;

            if (_mode == Mode.ThreadLoop)
            {
                Profiler.Function("waiting for render");
                if (_render.WaitOne(MaxFrameDuration))
                {
#if LOG_FRAME_SKIP
                    _lastFrameSkipped = false;
#endif

                    _render.Reset();
                    Profiler.Function("signaling render");
                    _main.Set();
                }
                else
                {
#if LOG_FRAME_SKIP
                    if (!_lastFrameSkipped)
                        _log.Trace("skipping frame(s)");
                    _lastFrameSkipped = true;
#endif

                    Profiler.Function("frame skip");
                }
            }
            else
                Update();
        }

        public TaskScheduler GetScheduler() => _scheduler;
    }
}
