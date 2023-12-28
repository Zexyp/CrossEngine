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
        readonly List<ISpriteRenderData> _sprites = new List<ISpriteRenderData>();

        public SpriteRendererSystem(SceneLayerRenderData layerRenderData)
        {
            layerRenderData.Data.Add((new SpriteRenderable(), _sprites));
        }

        public override void Register(SpriteRendererComponent component)
        {
            if (component.Enabled)
                _sprites.Add(component);
            component.EnabledChanged += OnComponentEnabledChanged;

            // sort based on blending so the we don't die of draw calls
            _sprites.Sort((a, b) => a.Blend.CompareTo(b.Blend));
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
