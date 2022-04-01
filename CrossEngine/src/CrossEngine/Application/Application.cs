using System;

using System.Diagnostics;

using CrossEngine.Display;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Logging;
using CrossEngine.Profiling;
using CrossEngine.Rendering;

namespace CrossEngine
{
    public abstract class Application
    {
        public static Application Instance { get; private set; } = null;

        RenderThread RenderThread;
        //public uint Width { get => Window.Width; set => Window.Width = value; }
        //public uint Height { get => Window.Height; set => Window.Height = value; }
        //public string Title { get => Window.Title; set => Window.Title = value; }
        public RendererAPI RendererAPI { get; private set; }

        private LayerStack LayerStack;
        //private double _fixedUpdateAggregate = 0;

#if PROFILING
        private bool _shouldStartProfiling = false;
        private bool _shouldEndProfiling = false;
#endif

        public Application(string title = "Window", int width = 1600, int height = 900)
        {
            // log needs initialization
            Log.Init();

            RenderThread = new RenderThread();

            if (Instance != null)
                Debug.Assert(false, "There can be only one Application!");

            Instance = this;

            LayerStack = new LayerStack();
        }

        public void Run()
        {
            //Profiler.BeginSession("session", "profiling.json");

            Init();

            LoadContent();

            OnEvent(new WindowResizeEvent(Window.Width, Window.Height));

            while (!(RenderThread.Window?.ShouldClose != false))
            {
#if PROFILING
                if (_shouldStartProfiling)
                {
                    _shouldStartProfiling = false;
                    Profiler.BeginSession("session", "profiling.json");
                }
#endif

                Profiler.BeginScope("Main loop");

                Time.Update(RenderThread.Window.Time);

                RenderThread.Run();

                Update();

                //EventLoop.Update();

                //Profiler.BeginScope(nameof(Render));
                //Render();
                //Profiler.EndScope();

                RenderThread.Join();

                JoinedUpdate();

                //_fixedUpdateAggregate += Time.UnscaledDeltaTime;
                //if (_fixedUpdateAggregate >= Time.FixedDeltaTime)
                //{
                //    _fixedUpdateAggregate = 0;
                //    Profiler.BeginScope(nameof(FixedUpdate));
                //    FixedUpdate();
                //    Profiler.EndScope();
                //}

                Input.Update();

                Event de;
                while ((de = RenderThread.DequeueEvent()) != null) OnEvent(de);

                Profiler.EndScope();

#if PROFILING
                if (_shouldEndProfiling)
                {
                    _shouldEndProfiling = false;
                    Profiler.EndSession();
                }
#endif
            }

            UnloadContent();

            Profiler.BeginScope(nameof(End));
            End();
            Profiler.EndScope();

            //Profiler.EndSession();
        }

        protected virtual void Init()
        {
            Profiler.BeginScope();
            RenderThread.Start();
            RenderThread.Join();

            RendererAPI = RenderThread.rapi;
            Profiler.EndScope();
        }

        protected virtual void End()
        {
            Profiler.BeginScope();
            RenderThread.Stop();
            RenderThread.Join();
            Profiler.EndScope();
        }

        protected virtual void LoadContent()
        {

        }

        protected virtual void UnloadContent()
        {
            LayerStack.PopAll();
            //GC.Collect();
            //Assets.GC.GPUGarbageCollector.Collect();
        }

        protected virtual void JoinedUpdate()
        {
            for (int i = 0; i < LayerStack.Layers.Count; i++)
            {
                LayerStack.Layers[i].OnJoinedUpdate();
            }
        }

        protected virtual void Update()
        {
            Profiler.BeginScope();
            for (int i = 0; i < LayerStack.Layers.Count; i++)
            {
                LayerStack.Layers[i].OnUpdate();
            }
            Profiler.EndScope();
        }

        public virtual void Render()
        {
            Profiler.BeginScope();
            for (int i = 0; i < LayerStack.Layers.Count; i++)
            {
                LayerStack.Layers[i].OnRender();
            }
            Profiler.EndScope();
        }

        //protected virtual void FixedUpdate()
        //{
        //    var layers = LayerStack.GetLayers();
        //    var fue = new FixedUpdateEvent();
        //    for (int i = 0; i < layers.Count; i++)
        //    {
        //        layers[i].OnEvent(fue);
        //    }
        //}

        public virtual void OnEvent(Event e)
        {
            Profiler.BeginScope($"{nameof(Application)}.{nameof(OnEvent)}({ e.GetType().Name})");

            var layers = LayerStack.Layers;
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                if (e.Handled) break;
                layers[i].OnEvent(e);
            }

            if (!e.Handled) Input.OnEvent(e);
            if (!e.Handled) GlobalEventDispatcher.Dispatch(e);

            Profiler.EndScope();
        }

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

        public Window Window => RenderThread.Window;

#if PROFILING
        public void StartProfiler() => _shouldStartProfiling = true;
        public void EndProfiler() => _shouldEndProfiling = true;
#endif
    }
}
