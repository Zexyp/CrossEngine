﻿using System;

using CrossEngine.Rendering.Display;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Logging;

namespace CrossEngine
{
    public abstract class Application
    {
        public static Application Instance { get; private set; } = null;

        internal Window Window { get; private set; }
        public uint Width => Window.Width;
        public uint Height => Window.Height;

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
            Init();

            LoadContent();

            OnEvent(new WindowResizeEvent(Window.Width, Window.Height));

            while (!Window.ShouldClose)
            {
                Time.Update(Window.Time);

                Update(Math.Min((float)Time.DeltaTime, MaxTimestep));

                Render();

                Input.Update();

                Window.Update();
            }

            UnloadContent();

            Window.CloseWindow();
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
            var layers = LayerStack.GetLayers();
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                if (e.Handled) break;
                layers[i].OnEvent(e);
            }

            if (!e.Handled) Input.OnEvent(e);

            GlobalEventDispatcher.Dispatch(e);
        }

        public void PushLayer(Layer layer) => LayerStack.PushLayer(layer);
        public void PushOverlay(Layer overlay) => LayerStack.PushOverlay(overlay);
        public void PopLayer(Layer layer) => LayerStack.PopLayer(layer);
        public void PopOverlay(Layer overlay) => LayerStack.PopOverlay(overlay);
    }
}
