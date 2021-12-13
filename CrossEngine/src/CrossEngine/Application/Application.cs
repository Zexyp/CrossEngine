using System;

using CrossEngine.Rendering.Display;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Logging;
using CrossEngine.Profiling;

namespace CrossEngine
{
    public abstract class Application
    {
        public static Application Instance { get; private set; } = null;

        internal Window Window { get; private set; }
        public uint Width { get => Window.Width; set => Window.Width = value; }
        public uint Height { get => Window.Height; set => Window.Height = value; }
        public string Title { get => Window.Title; set => Window.Title = value; }

        private LayerStack LayerStack;

        public Application(string title = "Window", int width = 1600, int height = 900)
        {
            Log.Init();

            WindowProperties props;
            props.title = title;
            props.width = (uint)width;
            props.height = (uint)height;

            if (Instance != null)
                System.Diagnostics.Debug.Assert(false, "There can be only one Application!");

            Instance = this;

            Window = new Window(props);

            LayerStack = new LayerStack();

            Window.SetEventCallback(OnEvent);
        }

        const float MaxTimestep = 1.0f / 60;

        public void Run()
        {
            Profiler.BeginSession("session", "profiling.json");

            Profiler.BeginScope(nameof(Init));
            Init();
            Profiler.EndScope();

            LoadContent();

            OnEvent(new WindowResizeEvent(Window.Width, Window.Height));

            while (!Window.ShouldClose)
            {
                Profiler.BeginScope("Main loop");
                Time.Update(Window.Time);

                Profiler.BeginScope(nameof(Update));
                Update(Math.Min((float)Time.DeltaTime, MaxTimestep));
                Profiler.EndScope();

                Profiler.BeginScope(nameof(Render));
                Render();
                Profiler.EndScope();

                Input.Update();

                Window.Update();
                Profiler.EndScope();
            }

            UnloadContent();

            Window.CloseWindow();

            Profiler.EndSession();
        }

        protected virtual void Init()
        {

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

        protected virtual void Update(float timestep)
        {
            var layers = LayerStack.GetLayers();
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnUpdate(timestep);
            }
        }

        protected virtual void Render()
        {
            var layers = LayerStack.GetLayers();
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnRender();
            }
        }

        protected virtual void OnEvent(Event e)
        {
            Profiler.BeginScope($"{nameof(OnEvent)}({ e.GetType().Name})");
            var layers = LayerStack.GetLayers();
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
    }
}
