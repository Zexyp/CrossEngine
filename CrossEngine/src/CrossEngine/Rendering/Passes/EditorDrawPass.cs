using CrossEngine.Rendering.Buffers;
using CrossEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Rendering.Lines;

namespace CrossEngine.Rendering.Passes
{
    public class EditorDrawPass : RenderPass
    {
        public override void Draw(Scene scene, Matrix4x4 viewProjectionMatrix, Framebuffer framebuffer = null)
        {
            LineRenderer.BeginScene(viewProjectionMatrix);

            scene.OnRender(new EditorDrawRenderEvent());
            
            LineRenderer.EndScene();
        }
    }
}
