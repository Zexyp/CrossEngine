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
    class SpriteRendererSystem : SimpleComponentSystem<SpriteRendererComponent>, IRenderableComponentSystem
    {
        public (IRenderable Renderable, IList Objects) RenderData { get; private set; }

        private List<ISpriteRenderData> _filtered = new List<ISpriteRenderData>();

        public SpriteRendererSystem() : base()
        {
            RenderData = (new SpriteRenderable(), _filtered);
        }

        public override void Register(SpriteRendererComponent component)
        {
            base.Register(component);

            EnabledChanged(component);
            component.OnEnabledChanged += EnabledChanged;
        }

        public override void Unregister(SpriteRendererComponent component)
        {
            base.Unregister(component);

            component.OnEnabledChanged -= EnabledChanged;
            _filtered.Remove(component);
        }

        private void EnabledChanged(Component sender)
        {
            if (sender.Enabled)
                _filtered.Add((SpriteRendererComponent)sender);
            else
                _filtered.Remove((SpriteRendererComponent)sender);
        }
    }
}
