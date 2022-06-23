using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.ComponentSystems
{
    class TextRendererSystem : System<TextRendererComponent>
    {
        List<ITextRenderData> _filtered = new List<ITextRenderData>();
        (IRenderable Renderable, IList Objects) Data;

        public TextRendererSystem(SceneLayerRenderData renderData) : base()
        {
            Data = (new TextRenderable(), _filtered);
            renderData.Data.Add(Data);
        }

        public override void Register(TextRendererComponent component)
        {
            base.Register(component);

            EnabledChange(component);
            component.OnEnabledChanged += EnabledChange;
        }

        public override void Unregister(TextRendererComponent component)
        {
            base.Unregister(component);

            component.OnEnabledChanged -= EnabledChange;
            _filtered.Remove(component);
        }

        private void EnabledChange(Component sender)
        {
            if (sender.Enabled)
                _filtered.Add((TextRendererComponent)sender);
            else
                _filtered.Remove((TextRendererComponent)sender);
        }
    }
}
