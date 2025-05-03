using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Scenes
{
    public static class SceneRenderer
    {
        public static void Render(Scene scene, ISurface surface)
        {
            Debug.Assert(scene.IsInitialized);
            
            surface.Context.Api.SetViewport(0, 0, (uint)surface.Size.X, (uint)surface.Size.Y);
            
            scene.World.GetSystem<RenderSystem>().Pipeline.Process(GraphicsContext.Current);
        }
    }
}
