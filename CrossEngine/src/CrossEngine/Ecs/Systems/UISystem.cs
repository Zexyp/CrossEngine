using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Systems
{
    class UIRenderable : Renderable<CanvasComponent>
    {
        public override void Begin(ICamera camera)
        {
            Renderer2D.BeginScene(camera.ViewProjectionMatrix);
        }

        public override void End()
        {
            Renderer2D.EndScene();
        }

        public override void Submit(CanvasComponent data)
        {
            foreach (var item in data.Entity.GetDeepComponents(typeof(UIComponent)))
            {
                ((UIComponent)item).Draw();
            }
        }
    }

    internal class UISystem : MulticastSystem<(CanvasComponent, UIComponent)>
    {
        private SceneRenderData renderData;
        private Dictionary<CanvasComponent, SceneLayerRenderData> layers = new Dictionary<CanvasComponent, SceneLayerRenderData>();

        public UISystem(SceneRenderData renderData)
        {
            this.renderData = renderData;
        }

        public override void Register(Component component)
        {
            if (component is CanvasComponent canvascomp)
            {
                var l = new SceneLayerRenderData();
                renderData.Layers.Add(l);
                l.Camera = canvascomp.camera;
                l.Data.Add((new UIRenderable(), new List<IObjectRenderData>() { canvascomp }));
                layers.Add(canvascomp, l);
            }
        }

        public override void Unregister(Component component)
        {
            if (component is CanvasComponent canvascomp)
            {
                var l = layers[canvascomp];
                renderData.Layers.Remove(l);
            }
        }
    }
}
