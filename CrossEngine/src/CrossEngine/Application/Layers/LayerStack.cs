using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using CrossEngine.Events;
using CrossEngine.Logging;

namespace CrossEngine.Layers
{
    public class LayerStack
    {
        private readonly List<Layer> _layers = new List<Layer>();
        public ReadOnlyCollection<Layer> Layers;

        int lastLayerIndex = 0;

        public LayerStack()
        {
            Layers = _layers.AsReadOnly();
        }

        public void PushLayer(Layer layer)
        {
            _layers.Insert(lastLayerIndex, layer);
            layer.OnAttach();
            lastLayerIndex++;
            layer.Attached = true;

            Application.CoreLog.Trace($"pushed layer ('{layer.GetType().Name}')");
        }

        public void PushOverlay(Layer overlay)
        {
            _layers.Add(overlay);
            overlay.OnAttach();
            overlay.Attached = true;

            Application.CoreLog.Trace($"pushed overlay ('{overlay.GetType().Name}')");
        }

        public void PopLayer(Layer layer)
        {
            layer.OnDetach();
            layer.Attached = false;
            _layers.Remove(layer);
            lastLayerIndex--;

            Application.CoreLog.Trace($"poped layer ('{layer.GetType().Name}')");
        }

        public void PopOverlay(Layer overlay)
        {
            overlay.OnDetach();
            overlay.Attached = false;
            _layers.Remove(overlay);

            Application.CoreLog.Trace($"poped overlay ('{overlay.GetType().Name}')");
        }

        public void PopAll()
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                _layers[i].OnDetach();
                _layers[i].Attached = false;

                Application.CoreLog.Trace($"poped layer/overlay ('{_layers[i].GetType().Name}')");
            }
            _layers.Clear();
            lastLayerIndex = 0;
        }
    }
}
