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
        public readonly ReadOnlyCollection<Layer> Layers;

        int _lastLayerIndex = 0;

        public LayerStack()
        {
            Layers = _layers.AsReadOnly();
        }

        public void PushLayer(Layer layer)
        {
            _layers.Insert(_lastLayerIndex, layer);
            layer.Attach();
            _lastLayerIndex++;

            Application.CoreLog.Trace($"pushed layer ('{layer.GetType().Name}')");
        }

        public void PushOverlay(Layer overlay)
        {
            _layers.Add(overlay);
            overlay.Attach();

            Application.CoreLog.Trace($"pushed overlay ('{overlay.GetType().Name}')");
        }

        public void PopLayer(Layer layer)
        {
            layer.Detach();
            _layers.Remove(layer);
            _lastLayerIndex--;

            Application.CoreLog.Trace($"popped layer ('{layer.GetType().Name}')");
        }

        public void PopOverlay(Layer overlay)
        {
            overlay.Detach();
            _layers.Remove(overlay);

            Application.CoreLog.Trace($"popped overlay ('{overlay.GetType().Name}')");
        }

        public void PopAll()
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                _layers[i].Detach();

                Application.CoreLog.Trace($"poped layer/overlay ('{_layers[i].GetType().Name}')");
            }
            _layers.Clear();
            _lastLayerIndex = 0;
        }

        public void PushLayersRange(IEnumerable<Layer> layers)
        {
            foreach (var item in layers)
            {
                PushLayer(item);
            }
        }

        public void PushOverlaysRange(IEnumerable<Layer> layers)
        {
            foreach (var item in layers)
            {
                PushOverlay(item);
            }
        }
    }
}
