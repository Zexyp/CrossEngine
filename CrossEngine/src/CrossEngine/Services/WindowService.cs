using CrossEngine.Display;
using CrossEngine.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CrossEngine.Services
{
    internal class WindowService : Service
    {
        public enum Mode
        {
            None = default,
            Manual,
            ThreadLoop,
        }

        public Window Window { get; private set; }
        public event OnEventFunction Event;
        public event Action WindowUpdate;

        Thread _windowThread;
        readonly ConcurrentQueue<Action> _execute = new ConcurrentQueue<Action>();
        Mode _mode;

        public WindowService(Mode mode)
        {
            _mode = mode;
        }

        public override void OnStart()
        {
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
            if (_mode == Mode.ThreadLoop)
            {
                _windowThread.Join();
            }
            else
                Destroy();
        }

        public void Execute(Action action)
        {
            _execute.Enqueue(action);
        }

        public void Update()
        {
            ExecuteQueued();

            WindowUpdate?.Invoke();
            Window.PollEvents();
        }

        private void Setup()
        {
            // setup
#if WINDOWS
            Window = new CrossEngine.Platform.Windows.GlfwWindow();
#elif WASM
            Window = new CrossEngine.Platform.Wasm.CanvasWindow();
#endif

            Window.Event += OnEvent;

            Window.Create();

            ExecuteQueued();
        }

        private void Destroy()
        {
            ExecuteQueued();

            Window.Event -= OnEvent;

            // destroy
            Window.Destroy();

            Window.Dispose();
        }

        private void OnEvent(Event e)
        {
            Event?.Invoke(e);
        }

        private void ExecuteQueued()
        {
            while (_execute.TryDequeue(out var result))
                result.Invoke();
        }

        private void Loop()
        {
            Setup();

            while (!Window.ShouldClose)
            {
                Update();
            }

            Destroy();
        }
    }
}
