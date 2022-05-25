using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering;

namespace CrossEngine.ComponentSystems
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
            throw new NotImplementedException();
        }

        public void UnregisterCanvas(CanvasComponent canvas)
        {
            throw new NotImplementedException();
        }
    }
}
