using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Scenes;
using CrossEngine.Entities.Components;
using CrossEngine.Rendering.Lines;

namespace CrossEngine.Rendering.Passes
{
    public class LineRenderPass : RenderPass
    {
        public override void Draw(Scene scene, Matrix4x4 viewProjectionMatrix, Framebuffer framebuffer = null)
        {
            if (framebuffer != null)
            {
                framebuffer.EnableColorDrawBuffer(scene.Pipeline.FbStructureIndex.Color, true);
                framebuffer.EnableColorDrawBuffer(scene.Pipeline.FbStructureIndex.Id, false);
            }

            Renderer.SetDepthFunc(DepthFunc.LessEqual);
            LineRenderer.BeginScene(viewProjectionMatrix);
            {
                scene.OnRender(new LineRenderEvent());
            }
            LineRenderer.EndScene();
            Renderer.SetDepthFunc(DepthFunc.Default);
        }
    }
}
