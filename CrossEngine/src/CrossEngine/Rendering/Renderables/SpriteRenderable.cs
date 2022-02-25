using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Components;
using CrossEngine.Utils;
using CrossEngine.Rendering.Textures;

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
        public override void Begin(Matrix4x4 viewProjectionMatrix)
        {
            Renderer2D.BeginScene(viewProjectionMatrix);
        }

        public override void End()
        {
            Renderer2D.EndScene();
        }

        public override void Submit(ISpriteRenderData data)
        {
            if (data == null) Logging.Log.Core.Error("something went wrong, again...");
            if (data.Texture == null)
                Renderer2D.DrawQuad(data.Transform, data.Color, data.EntityId);
            else
                Renderer2D.DrawTexturedQuad(data.Transform, data.Texture, data.Color, data.TextureOffsets, data.EntityId);
        }
    }
}
