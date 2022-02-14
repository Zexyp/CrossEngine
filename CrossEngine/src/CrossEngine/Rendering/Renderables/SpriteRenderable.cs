using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Components;

namespace CrossEngine.Rendering.Renderables
{
    class SpriteRenderable : Renderable
    {
        public override void Begin()
        {
            Renderer2D.BeginScene(Matrix4x4.Identity);
        }

        public override void End()
        {
            Renderer2D.EndScene();
        }

        public override void Submit(IObjectRenderData data)
        {
            var src = (SpriteRendererComponent)data;
            if (src.Entity.TryGetComponent(out TransformComponent tc))
                Renderer2D.DrawQuad(tc.WorldTransformMatrix, src.Color);
        }
    }
}
