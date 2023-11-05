using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CrossEngine.Display;
using CrossEngine.Rendering;
using CrossEngine.Profiling;
using CrossEngine.Services;
using CrossEngine.Events;

#if WINDOWS
using CrossEngine.Platform.Windows;
#endif

namespace CrossEngine.Services
{
    internal class RenderService : Service
    {
        public RendererAPI RendererAPI { get; private set; }
        public Window Window { get; private set; }
        public event Action Frame;
        public event Action BeforeFrame;
        public event Action AfterFrame;

        ConcurrentQueue<Action> _execute = new ConcurrentQueue<Action>();
        Thread _renderThread;
        bool _running = false;

        public override void OnStart()
        {
#if WINDOWS
            _renderThread = new Thread(Loop);
            _running = true;
            _renderThread.Start();
#elif WASM
            Setup();
#endif
        }

        public override void OnDestroy()
        {
#if WINDOWS
            _running = false;
            _renderThread.Join();
#elif WASM
            Destroy();
#endif
        }

        public void Execute(Action action)
        {
            _execute.Enqueue(action);
        }

        private void Setup()
        {
            // setup
            RendererAPI = RendererAPI.Create(RendererAPI.API.OpenGLES);

#if WINDOWS
            Window = new CrossEngine.Platform.Windows.GlfwWindow();
#elif WASM
            Window = new CrossEngine.Platform.Wasm.CanvasWindow();
#endif

            Window.OnEvent += Window_OnEvent;

            Window.CreateWindow();

            RendererAPI.Init();
            RendererAPI.SetClearColor(new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1.0f));
        }

        // this is supposed to fix state when resizing window
        // it's unfortunate that when just holding the window nothing happens
        private void Window_OnEvent(Event e)
        {
            if (e is WindowRefreshEvent)
                Draw();
        }

        private void Destroy()
        {
            Window.OnEvent -= Window_OnEvent;

            // destroy
            Window.DestroyWindow();

            Window.Dispose();

            RendererAPI.Dispose();
        }

        private void Draw()
        {
            Profiler.BeginScope("Render");

            while (_execute.TryDequeue(out var result))
                result.Invoke();

            BeforeFrame?.Invoke();
            Frame?.Invoke();
            AfterFrame?.Invoke();

            Profiler.EndScope();

            Profiler.BeginScope("Swap");

            Window.Context.SwapBuffers();

            Profiler.EndScope();
        }

        private void Loop()
        {
            Setup();

            while (_running)
            {
                Draw();

                Profiler.BeginScope("Poll Events");

                Window.PollWindowEvents();

                Profiler.EndScope();
            }

            Destroy();
        }
    }
}
