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
        virtual Vector4 DrawOffsets => new Vector4(0, 0, 1, 1);
    }

    class SpriteRenderable : Renderable<ISpriteRenderData>
    {
        private BlendMode? _lastBlend = null;

        public override void Begin(ICamera camera)
        {
            Renderer2D.BeginScene(((ICamera)camera).GetViewProjectionMatrix());
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

            var matrix = Matrix4x4.CreateScale(new Vector3(data.DrawOffsets.Z, data.DrawOffsets.W, 1)) * Matrix4x4.CreateTranslation(new Vector3(data.DrawOffsets.X, data.DrawOffsets.Y, 1)) * data.Transform;
            if (data.Texture == null)
                Renderer2D.DrawQuad(matrix, data.Color, data.EntityId);
            else
                Renderer2D.DrawTexturedQuad(matrix, data.Texture, data.Color, data.TextureOffsets, data.EntityId);
        }
    }
}
