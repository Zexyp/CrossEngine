using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Components;
using CrossEngine.Utils;
using CrossEngine.Rendering.Textures;
using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering.Renderables
{
    public interface ISpriteRenderData : IObjectRenderData
    {
        Vector4 Color { get; }
        virtual Ref<Texture> Texture { get => null; }
        virtual int EntityId { get => 0; }
        virtual Vector4 TextureOffsets { get => new Vector4(0, 0, 1, 1); }
    }

    class SpriteRenderable : Renderable<ISpriteRenderData>
    {
        public override void Begin(Camera camera)
        {
            Renderer2D.Flush();
            Application.Instance.RendererAPI.SetBlendFunc(BlendFunc.OneMinusSrcAlpha);
        }

        public override void Submit(ISpriteRenderData data)
        {
            if (data.Texture == null)
                Renderer2D.DrawQuad(data.Transform, data.Color, data.EntityId);
            else
                Renderer2D.DrawTexturedQuad(data.Transform, data.Texture, data.Color, data.TextureOffsets, data.EntityId);
        }
    }
}
