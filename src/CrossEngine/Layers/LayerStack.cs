using System;
using System.Collections.Generic;
using System.Text;

namespace CrossEngine.Layers
{
    public class LayerStack
    {
        List<Layer> layers = new List<Layer> { };

        int firstOverlayIndex = 0;

        bool awake = false;

        public void PushLayer(Layer layer)
        {
            layers.Add(layer);
            if (awake) layer.OnAttach();
            firstOverlayIndex++;
        }

        public void PushOverlay(Layer overlay)
        {
            layers.Insert(firstOverlayIndex, overlay);
            if (awake) overlay.OnAttach();
        }

        public void PopLayer(Layer layer)
        {
            layer.OnDetach();
            layers.Remove(layer);
            firstOverlayIndex--;
        }

        public void PopOverlay(Layer overlay)
        {
            overlay.OnDetach();
            layers.Remove(overlay);
        }

        public void Update(float timestep)
        {
            foreach (Layer layer in layers)
            {
                layer.OnUpdate(timestep);
            }
        }

        public void Render()
        {
            foreach (Layer layer in layers)
            {
                layer.OnRender();
            }
        }

        public void Awake()
        {
            awake = true;

            foreach(Layer layer in layers)
            {
                layer.OnAttach();
            }
        }

        public void Die()
        {
            awake = false;

            foreach (Layer layer in layers)
            {
                layer.OnDetach();
            }
        }
    }
}
