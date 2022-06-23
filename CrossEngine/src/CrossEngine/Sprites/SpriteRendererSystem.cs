using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Profiling;

namespace CrossEngine.ComponentSystems
{
    class SpriteRendererSystem : System<SpriteRendererComponent>
    {
        List<ISpriteRenderData> _filtered = new List<ISpriteRenderData>();
        (IRenderable Renderable, IList Objects) Data;

        public SpriteRendererSystem(SceneLayerRenderData renderData) : base()
        {
            Data = (new SpriteRenderable(), _filtered);
            renderData.Data.Add(Data);
        }

        public override void Register(SpriteRendererComponent component)
        {
            base.Register(component);

            EnabledChange(component);
            component.OnEnabledChanged += EnabledChange;
        }

        public override void Unregister(SpriteRendererComponent component)
        {
            base.Unregister(component);

            component.OnEnabledChanged -= EnabledChange;
            _filtered.Remove(component);
        }

        private void EnabledChange(Component sender)
        {
            if (sender.Enabled)
                _filtered.Add((SpriteRendererComponent)sender);
            else
                _filtered.Remove((SpriteRendererComponent)sender);
        }
    }
}
