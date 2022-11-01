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

namespace CrossEngine.Systems
{
    class TextRendererSystem : SimpleSystem<TextRendererComponent>, IRenderableSystem
    {
        public (IRenderable Renderable, IList Objects) RenderData { get; private set; }
        
        private List<ITextRenderData> _filtered = new List<ITextRenderData>();

        public TextRendererSystem() : base()
        {
            RenderData = (new TextRenderable(), _filtered);
        }

        public override void Register(TextRendererComponent component)
        {
            base.Register(component);

            Component_OnEnabledChanged(component);
            component.OnEnabledChanged += Component_OnEnabledChanged;
        }

        public override void Unregister(TextRendererComponent component)
        {
            base.Unregister(component);

            component.OnEnabledChanged -= Component_OnEnabledChanged;
            _filtered.Remove(component);
        }

        private void Component_OnEnabledChanged(Component sender)
        {
            if (sender.Enabled)
                _filtered.Add((TextRendererComponent)sender);
            else
                _filtered.Remove((TextRendererComponent)sender);
        }
    }
}
