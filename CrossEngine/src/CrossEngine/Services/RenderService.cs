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
using CrossEngine.Platform.OpenGL;
using CrossEngine.Platform.Windows;

namespace CrossEngine.Services
{
    internal class RenderService : Service
    {
        public RendererAPI RendererAPI { get; private set; }
        public GraphicsContext Context { get; private set; }
        public Window Window { get; private set; }
        public event Action Frame;

        ConcurrentQueue<Action> _execute = new ConcurrentQueue<Action>();
        Thread _renderThread;
        bool _running = false;

        public override void OnStart()
        {
            _renderThread = new Thread(Loop);
            _running = true;
            _renderThread.Start();
        }

        public override void OnDestroy()
        {
            _running = false;
            _renderThread.Join();
        }

        public void Execute(Action action)
        {
            _execute.Enqueue(action);
        }

        private void Loop()
        {
            // setup
            RendererAPI = RendererAPI.Create(RendererAPI.API.OpenGL);
            Window = new CrossEngine.Platform.Windows.GlfwWindow();

            Window.CreateWindow();

            Context = new CrossEngine.Platform.OpenGL.GLContext(((GlfwWindow)Window).NativeHandle);

            Context.Init();

            RendererAPI.Init();
            RendererAPI.SetClearColor(new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1.0f));

            while (_running)
            {
                Profiler.BeginScope("Render");

                while (_execute.TryDequeue(out var result))
                    result.Invoke();

                Frame?.Invoke();

                Profiler.EndScope();
                Profiler.BeginScope("Swap");

                Context.SwapBuffers();
                
                Profiler.EndScope();
                Profiler.BeginScope("Poll Events");

                Window.PollWindowEvents();

                Profiler.EndScope();
            }

            // destroy
            Window.DestroyWindow();

            Window.Dispose();
            
            RendererAPI.Dispose();
        }
    }
}
