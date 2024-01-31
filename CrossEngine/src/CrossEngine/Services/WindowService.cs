using CrossEngine.Display;
using CrossEngine.Events;
using CrossEngine.Platform;
using CrossEngine.Profiling;
using CrossEngine.Utils;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CrossEngine.Services
{
    public class WindowService : Service, IScheduledService, IUpdatedService
    {
        public enum Mode
        {
            None = default,
            Sync,
            ThreadLoop,
        }

        public Window Window { get; private set; }
        public event Action<WindowService, Event> WindowEvent;
        public event Action<WindowService> WindowUpdate;
        public int MaxFrameDuration = (int)(1d / 30 * 1000);

        Thread _windowThread;
        readonly SingleThreadedTaskScheduler _scheduler = new SingleThreadedTaskScheduler();
        Mode _mode;
        bool _running = false;
        AutoResetEvent _render = new AutoResetEvent(false);
        AutoResetEvent _main = new AutoResetEvent(false);

        public WindowService(Mode mode)
        {
            _mode = mode;
        }

        public override void OnStart()
        {
            _running = true;
            if (_mode == Mode.ThreadLoop)
            {
                _windowThread = new Thread(Loop);
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

        public void Update()
        {
            _scheduler.RunOnCurrentThread();

            WindowUpdate?.Invoke(this);
            Window.Keyboard.Update();
            Window.Mouse.Update();

            Profiler.BeginScope($"{nameof(CrossEngine.Display.Window)}.{nameof(CrossEngine.Display.Window.PollEvents)}");
            Window.PollEvents();
            Profiler.EndScope();
        }

        private void Setup()
        {
            // setup
            Window = PlatformHelper.CreateWindow();

            Window.Event += OnWindowEvent;

            Window.Create();

            _scheduler.RunOnCurrentThread();
        }

        private void Destroy()
        {
            _scheduler.RunOnCurrentThread();

            Window.Event -= OnWindowEvent;

            // destroy
            Window.Destroy();

            Window.Dispose();
        }

        private void OnWindowEvent(Event e)
        {
            WindowEvent?.Invoke(this, e);

            Manager.Event(e);
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
        bool _lastFrameSkipped = false;
        public void OnUpdate()
        {
            if (!_running)
                return;

            if (_mode == Mode.ThreadLoop)
            {
                Profiler.Function("waiting for render");
                if (_render.WaitOne(MaxFrameDuration))
                {
                    _lastFrameSkipped = false;
                    _render.Reset();
                    Profiler.Function("signaling render");
                    _main.Set();
                }
                else
                {
                    if (!_lastFrameSkipped) ;//Logging.Log.Default.Trace("skipping frame(s)");
                    _lastFrameSkipped = true;
                    Profiler.Function("frame skip");
                }
            }
            else
                Update();
        }

        public TaskScheduler GetScheduler() => _scheduler;
    }
}
