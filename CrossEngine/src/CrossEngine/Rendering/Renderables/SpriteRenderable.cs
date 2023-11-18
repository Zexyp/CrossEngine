using CrossEngine.Rendering.Cameras;
using System.Numerics;

namespace CrossEngine.Rendering.Renderables
{
    interface ISpriteRenderData : IObjectRenderData
    {
        Vector4 Color { get; }
        virtual int EntityId => 0;
        /*virtual Vector4 TextureOffsets => new Vector4(0, 0, 1, 1);*/
    }

    class SpriteRenderable : Renderable<ISpriteRenderData>
    {
        public override void Begin(ICamera camera)
        {
            Renderer2D.BeginScene(camera.ViewProjectionMatrix);
        }

        public override void End()
        {
            Renderer2D.EndScene();
        }

        public override void Submit(ISpriteRenderData data)
        {
            Renderer2D.DrawQuad(data.Transform, data.Color, data.EntityId);
        }
    }
}
