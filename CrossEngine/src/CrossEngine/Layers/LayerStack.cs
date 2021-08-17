using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using CrossEngine.Events;

namespace CrossEngine.Layers
{
    public class LayerStack
    {
        private List<Layer> _layers = new List<Layer> { };

        int lastLayerIndex = 0;

        public void PushLayer(Layer layer)
        {
            _layers.Insert(lastLayerIndex, layer);
            layer.OnAttach();
            lastLayerIndex++;
        }

        public void PushOverlay(Layer overlay)
        {
            _layers.Add(overlay);
            overlay.OnAttach();
        }

        public void PopLayer(Layer layer)
        {
            layer.OnDetach();
            _layers.Remove(layer);
            lastLayerIndex--;
        }

        public void PopOverlay(Layer overlay)
        {
            overlay.OnDetach();
            _layers.Remove(overlay);
        }

        public void PopAll()
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                _layers[i].OnDetach();
            }
            _layers.Clear();
            lastLayerIndex = 0;
        }

        public ReadOnlyCollection<Layer> GetLayers() => _layers.AsReadOnly();
    }
}
