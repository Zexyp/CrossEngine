using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Services;
using CrossEngine.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Components;
using CrossEngine.Rendering;

namespace CrossEngineEditor.Panels
{
    internal class GamePanel : SceneViewPanel
    {
        protected override ICamera DrawCamera => null;
        protected override Scene Scene => Context.Scene;

        public GamePanel(RenderService rs) : base(rs)
        {
            WindowName = "Game";
            Surface.Resize += OnSurfaceResize;
        }

        private void OnSurfaceResize(ISurface surface, float width, float height)
        {
            if (Scene?.IsInitialized == true)
                Scene.World.GetSystem<RenderSystem>().OnSurfaceResize(surface, width, height);
        }
    }
}
