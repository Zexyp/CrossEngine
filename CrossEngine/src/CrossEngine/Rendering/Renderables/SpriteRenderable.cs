using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Textures;
using System;
using System.Numerics;
using CrossEngine.Rendering.Culling;
using CrossEngine.Utils;
using CrossEngine.Utils.Rendering;

namespace CrossEngine.Rendering.Renderables
{
    class SpriteRenderable : Renderable<ISpriteRenderData>
    {
        private BlendMode? _lastBlend = null;

        public override void Begin(ICamera camera)
        {
            _lastBlend = null;
            Renderer2D.BeginScene(((ICamera)camera).GetViewProjectionMatrix());
        }

        public override void End()
        {
            Renderer2D.EndScene();
        }

        public override void Submit(ISpriteRenderData data)
        {            
            if (_lastBlend != data.Blend)
            {
                _lastBlend = data.Blend;
                Renderer2D.Flush();
                Renderer2D.SetBlending(data.Blend);
            }

            var offsets = data.DrawOffsets;
            var transform = Matrix4x4.CreateScale(new Vector3(offsets.Z, offsets.W, 1)) *
                            Matrix4x4.CreateTranslation(new Vector3(offsets.X, offsets.Y, 0)) * data.Transform;
            if (data.Texture == null)
                Renderer2D.DrawQuad(transform, data.Color, data.Id);
            else
                Renderer2D.DrawTexturedQuad(transform, data.Texture, data.Color, data.TextureOffsets, data.Id);
        }
    }
}
