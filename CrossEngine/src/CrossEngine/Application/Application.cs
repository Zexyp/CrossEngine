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
        public Window Window => _renderThread?.Window;
        public RendererAPI RendererAPI { get; private set; }

        private readonly RenderThread _renderThread;
        private readonly LayerStack LayerStack;
        private ConcurrentQueue<Event> _events = new ConcurrentQueue<Event>();
        //private double _fixedUpdateAggregate = 0;

        public Application(string title = "Window", int width = 1600, int height = 900)
        {
            CoreLog = new Logger("CORE");
            Log = new Logger("APP");

            RendererAPI = RendererAPI.Create();
            _renderThread = new RenderThread(RendererAPI);
            _renderThread.OnEvent += (e) =>
            {
                Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.OnEvent)}");
                OnEvent(e);
                Profiler.EndScope();
            };
            _renderThread.OnRender += () =>
            {
                Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.Render)}");
                Render();
                Profiler.EndScope();
            };
            _renderThread.OnInit += RenderInit;
            _renderThread.OnDestroy += RenderDestry;

            if (Instance != null)
                Debug.Assert(false, "There can be only one Application!");

            Instance = this;

            LayerStack = new LayerStack();

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

            OnEvent(new WindowResizeEvent(Window.Width, Window.Height));

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

                while (ThreadManager.MainThreadActionQueue.TryDequeue(out Action action))
                    action.Invoke();

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

                //Event de;
                //while ((de = _renderThread.DequeueEvent()) != null)
                //    OnEvent(de);

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

        protected virtual void RenderInit() { }

        protected virtual void Destroy()
        {
            _renderThread.Stop();
            _renderThread.Wait();
        }

        protected virtual void RenderDestry() { }

        protected virtual void LoadContent()
        {

        }

        protected virtual void UnloadContent()
        {
            LayerStack.PopAll();
            //GC.Collect();
            //Assets.GC.GPUGarbageCollector.Collect();
        }

        protected virtual void Update()
        {
            for (int i = 0; i < LayerStack.Layers.Count; i++)
            {
                LayerStack.Layers[i].OnUpdate();
            }
        }

        public virtual void Render()
        {
            for (int i = 0; i < LayerStack.Layers.Count; i++)
            {
                LayerStack.Layers[i].OnRender();
            }
        }

        public virtual void OnEvent(Event e)
        {
            Profiler.BeginScope($"{nameof(Application)}.{nameof(Application.OnEvent)}({e.GetType().Name})(base)");

            var layers = LayerStack.Layers;
            lock (layers)
            {
                for (int i = layers.Count - 1; i >= 0; i--)
                {
                    if (e.Handled) break;
                    layers[i].OnEvent(e);
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
        public void PushLayer(Layer layer) => LayerStack.PushLayer(layer);
        public void PushOverlay(Layer overlay) => LayerStack.PushOverlay(overlay);

        public void PopLayer(Layer layer) => LayerStack.PopLayer(layer);
        public void PopOverlay(Layer overlay) => LayerStack.PopOverlay(overlay);

        public T GetLayer<T>() where T : Layer
        {
            for (int i = 0; i < LayerStack.Layers.Count; i++)
            {
                if (LayerStack.Layers[i] is T) return (T)LayerStack.Layers[i];
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
