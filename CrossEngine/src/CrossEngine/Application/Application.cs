using System;

using System.Diagnostics;
using System.Threading;

using CrossEngine.Display;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Logging;
using CrossEngine.Profiling;
using CrossEngine.Rendering;
using System.Collections.Concurrent;

namespace CrossEngine
{
    public abstract class Application
    {
        internal static Logger CoreLog;
        public static Logger Log;

        public static Application Instance { get; private set; } = null;
        public RendererAPI RendererAPI { get; private set; }
        public Window Window => _renderThread?.Window;

        private readonly RenderThread _renderThread;
        private readonly LayerStack _layerStack = new LayerStack();
        private ConcurrentQueue<Event> _events = new ConcurrentQueue<Event>();
        //private double _fixedUpdateAggregate = 0;

        public Application(string title = "Window", int width = 1600, int height = 900)
        {
            if (Instance != null)
                Debug.Assert(false, "There can be only one Application!");
            Instance = this;

            // logs
            CoreLog = new Logger("CORE");
            Log = new Logger("APP");

            // render thread
            RendererAPI = RendererAPI.Create();
            _renderThread = new RenderThread(RendererAPI);
            _renderThread.OnEvent += (e) =>
            {
                Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Event)}");
                Event(e);
                Profiler.EndScope();
            };
            _renderThread.OnRender += () =>
            {
                Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Render)}");
                Render();
                Profiler.EndScope();
            };
            _renderThread.OnInit += RenderInit;
            _renderThread.OnDestroy += RenderDestroy;

            ThreadManager.SetMainThread(Thread.CurrentThread);
            ThreadManager.ConfigureCurrentThread();
        }

        public void Run()
        {
            Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Init)}");
            Init();
            Profiler.EndScope();

            Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.LoadContent)}");
            LoadContent();
            Profiler.EndScope();

            Event(new WindowResizeEvent(Window.Width, Window.Height));

            while (!(Window?.ShouldClose != false))
            {
#if PROFILING
                if (_shouldStartProfiling)
                {
                    _shouldStartProfiling = false;
                    Profiler.BeginSession("session", "profiling.json");
                }
#endif

                Profiler.BeginScope("Main loop");

                _renderThread.Begin();

                Time.Update(Window.Time);

                ThreadManager.ProcessMainThread();

                Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Update)}");
                Update();
                Profiler.EndScope();

                //EventLoop.Update();

                //_fixedUpdateAggregate += Time.UnscaledDeltaTime;
                //if (_fixedUpdateAggregate >= Time.FixedDeltaTime)
                //{
                //    _fixedUpdateAggregate = 0;
                //    Profiler.BeginScope(nameof(FixedUpdate));
                //    FixedUpdate();
                //    Profiler.EndScope();
                //}

                Input.Update();

                _renderThread.Wait();

                Profiler.EndScope();

#if PROFILING
                if (_shouldEndProfiling)
                {
                    _shouldEndProfiling = false;
                    Profiler.EndSession();
                }
#endif
            }

            Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.UnloadContent)}");
            UnloadContent();
            Profiler.EndScope();

            Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Destroy)}");
            Destroy();
            Profiler.EndScope();
        }

        #region Virtual Methods
        protected virtual void Init()
        {
            _renderThread.Start();

            _renderThread.Wait();
        }

        protected virtual void RenderInit()
        {
            Event(new ApplicationRenderInitEvent());

            var layers = _layerStack.Layers;
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].RenderAttach();
            }
        }

        protected virtual void Destroy()
        {
            _renderThread.Stop();
            _renderThread.Wait();

            _layerStack.PopAll();
            //GC.Collect();
            //Assets.GC.GPUGarbageCollector.Collect();
        }

        protected virtual void RenderDestroy()
        {
            var layers = _layerStack.Layers;
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].RenderDetach();
            }

            Event(new ApplicationRenderDestroyEvent());
        }

        protected virtual void LoadContent()
        {

        }

        protected virtual void UnloadContent()
        {
            
        }

        protected virtual void Update()
        {
            var layers = _layerStack.Layers;
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Update();
            }
        }

        public virtual void Render()
        {
            var layers = _layerStack.Layers;
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Render();
            }
        }

        public virtual void Event(Event e)
        {
            Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Event)}({e.GetType().Name})(base)");

            var layers = _layerStack.Layers;
            lock (layers)
            {
                for (int i = layers.Count - 1; i >= 0; i--)
                {
                    if (e.Handled) break;
                    layers[i].Event(e);
                }
            }

            if (!e.Handled) Input.OnEvent(e);
            if (!e.Handled) GlobalEventDispatcher.Dispatch(e);

            if (e is WindowCloseEvent && !e.Handled)
            {
                Window.ShouldClose = true;
            }

            Profiler.EndScope();
        }
        #endregion

        #region Layer Operations Methods
        public void PushLayer(Layer layer)
        {
            _layerStack.PushLayer(layer);
            if (_renderThread.Running) ThreadManager.ExecuteOnRenderThread(layer.RenderAttach);
        }
        public void PushOverlay(Layer overlay)
        {
            _layerStack.PushOverlay(overlay);
            if (_renderThread.Running) ThreadManager.ExecuteOnRenderThread(overlay.RenderAttach);
        }

        public void PopLayer(Layer layer)
        {
            if (_renderThread.Running) ThreadManager.ExecuteOnRenderThread(layer.RenderDetach);
            _layerStack.PopLayer(layer);
        }
        public void PopOverlay(Layer overlay)
        {
            if (_renderThread.Running) ThreadManager.ExecuteOnRenderThread(overlay.RenderDetach);
            _layerStack.PopOverlay(overlay);
        }

        public T GetLayer<T>() where T : Layer
        {
            var layers = _layerStack.Layers;
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i] is T) return (T)layers[i];
            }
            return null;
        }
        #endregion

        #region Profiling Methods
#if PROFILING
        private bool _shouldStartProfiling = false;
        private bool _shouldEndProfiling = false;
#endif

        [Conditional("PROFILING")]
        public void StartProfiler()
        {
#if PROFILING
            _shouldStartProfiling = true;
#endif
        }
        [Conditional("PROFILING")]
        public void EndProfiler()
        {
#if PROFILING
            _shouldEndProfiling = true;
#endif
        }
        #endregion
    }
}
