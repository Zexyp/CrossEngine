using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;

using CrossEngine.Scenes;
using CrossEngine.Entities.Components;
using CrossEngine.Rendering.Lines;

namespace CrossEngine.Rendering.Passes
{
    public class LineRenderPassEvent : RenderPassEvent
    {
        
    }

    public class LineRenderPass : RenderPass
    {
        public override void Render(SceneData data)
        {
            Renderer.SetDepthFunc(DepthFunc.LessEqual);
            LineRenderer.BeginScene(data.ViewProjectionMatrix);
            data.Scene.OnRender(new LineRenderPassEvent());
            LineRenderer.EndScene();
            Renderer.SetDepthFunc(DepthFunc.Default);
        }
    }
}
