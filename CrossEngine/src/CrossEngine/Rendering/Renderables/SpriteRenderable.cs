using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Textures;
using System;
using System.Numerics;

namespace CrossEngine.Rendering.Renderables
{
    interface ISpriteRenderData : IObjectRenderData
    {
        Vector4 Color { get; }
        virtual int EntityId => 0;
        virtual Vector4 TextureOffsets => new Vector4(0, 0, 1, 1);
        virtual WeakReference<Texture> Texture => null;
        virtual BlendMode Blend => BlendMode.Opaque;
        virtual Vector4 DrawOffsets => throw new NotImplementedException();
    }

    class SpriteRenderable : Renderable<ISpriteRenderData>
    {
        private BlendMode? _lastBlend = null;

        public override void Begin(ICamera camera)
        {
            Renderer2D.BeginScene(camera.ViewProjectionMatrix);
        }

        public override void End()
        {
            Renderer2D.EndScene();
            _lastBlend = null;
        }

        public override void Submit(ISpriteRenderData data)
        {
            if (_lastBlend != data.Blend)
            {
                _lastBlend = data.Blend;
                Renderer2D.Flush();
                Renderer2D.SetBlending(data.Blend);
            }

            if (data.Texture == null)
                Renderer2D.DrawQuad(data.Transform, data.Color, data.EntityId);
            else
                Renderer2D.DrawTexturedQuad(data.Transform, data.Texture, data.Color, data.TextureOffsets, data.EntityId);
        }
    }
}
