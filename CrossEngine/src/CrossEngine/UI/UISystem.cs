using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Events;

namespace CrossEngine.Systems
{
    class UISystem : ISystem
    {
        public SystemThreadMode ThreadMode => SystemThreadMode.Sync;

        SceneRenderData _renderData;

        Dictionary<CanvasComponent, SceneLayerRenderData> _layers = new Dictionary<CanvasComponent, SceneLayerRenderData>();

        public UISystem(SceneRenderData renderData)
        {
            _renderData = renderData;
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void RegisterCanvas(CanvasComponent canvas)
        {
            var lrd = new SceneLayerRenderData();
            lrd.Camera = canvas.Camera;
            _renderData.Layers.Add(lrd);
            _layers.Add(canvas, lrd);
        }

        public void UnregisterCanvas(CanvasComponent canvas)
        {
            _renderData.Layers.Remove(_layers[canvas]);
            _layers.Remove(canvas);
        }

        void ISystem.Event(object e)
        {
            Event cee = e as Event;
            foreach (var item in _layers.Keys)
            {
                item.OnEvent(cee);
            }
        }
    }
}
