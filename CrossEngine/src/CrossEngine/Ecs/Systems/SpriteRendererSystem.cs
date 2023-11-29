using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Renderables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Systems
{
    internal class SpriteRendererSystem : UnicastSystem<SpriteRendererComponent>
    {
        readonly SceneLayerRenderData _layerRenderData = new SceneLayerRenderData();
        readonly List<ISpriteRenderData> _sprites = new List<ISpriteRenderData>();

        public SpriteRendererSystem(SceneLayerRenderData layerRenderData)
        {
            _layerRenderData = layerRenderData;
            _layerRenderData.Data.Add((new SpriteRenderable(), _sprites));
        }

        public override void Attach()
        {
            base.Attach();

            World.GetSystem<RenderSystem>().PrimaryCameraChanged += (rsys) => { _layerRenderData.Camera = rsys.PrimaryCamera; };
        }

        public override void Register(SpriteRendererComponent component)
        {
            if (component.Enabled)
                _sprites.Add(component);
            component.EnabledChanged += OnComponentEnabledChanged;
        }

        public override void Unregister(SpriteRendererComponent component)
        {
            component.EnabledChanged -= OnComponentEnabledChanged;
            if (component.Enabled)
                _sprites.Remove(component);
        }

        private void OnComponentEnabledChanged(Component component)
        {
            var src = (ISpriteRenderData)component;
            if (component.Enabled)
                _sprites.Add(src);
            else
                _sprites.Remove(src);
        }
    }
}
