using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Textures;
using System;
using System.Numerics;

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

            var matrix = Matrix4x4.CreateScale(new Vector3(data.DrawOffsets.Z, data.DrawOffsets.W, 1)) * Matrix4x4.CreateTranslation(new Vector3(data.DrawOffsets.X, data.DrawOffsets.Y, 0)) * data.Transform;
            if (data.Texture == null)
                Renderer2D.DrawQuad(matrix, data.Color, data.Id);
            else
                Renderer2D.DrawTexturedQuad(matrix, data.Texture, data.Color, data.TextureOffsets, data.Id);
        }
    }
}
