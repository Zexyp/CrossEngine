using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

using CrossEngine.Scenes;
using CrossEngine.Entities.Components;

namespace CrossEngine.Rendering.Passes
{
    public class SpriteRenderPassEvent : RenderPassEvent
    {
        public readonly TransparencyMode TransparencyMode;

        public SpriteRenderPassEvent(TransparencyMode transparencyMode)
        {
            TransparencyMode = transparencyMode;
        }
    }

    public class SpriteRenderPass : RenderPass
    {
        public override void Render(SceneData data)
        {
            Renderer2D.BeginScene(data.ViewProjectionMatrix);
            {
                SpriteRenderPassEvent re = new SpriteRenderPassEvent(TransparencyMode.None);
                data.Scene.OnRender(re);
            }
            Renderer2D.EndScene();

            Renderer2D.EnableDiscardingTransparency(true);

            Renderer2D.BeginScene(data.ViewProjectionMatrix);
            {
                SpriteRenderPassEvent re = new SpriteRenderPassEvent(TransparencyMode.Discarding);
                data.Scene.OnRender(re);
            }
            Renderer2D.EndScene();

            Renderer2D.EnableDiscardingTransparency(false);

            Renderer.EnableBlending(true, BlendFunc.OneMinusSrcAlpha);

            Renderer2D.BeginScene(data.ViewProjectionMatrix);
            {
                SpriteRenderPassEvent re = new SpriteRenderPassEvent(TransparencyMode.Blending);
                data.Scene.OnRender(re);
            }
            Renderer2D.EndScene();

            Renderer.EnableBlending(false);
        }
    }
}
