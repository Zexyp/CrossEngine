using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Services;
using CrossEngine.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Panels
{
    internal class GamePanel : SceneViewPanel
    {
        protected override ICamera DrawCamera => null;
        protected override Scene Scene => Context.Scene;

        public GamePanel(RenderService rs) : base(rs)
        {
            WindowName = "Game";
        }

        protected override void DrawWindowContent()
        {
            base.DrawWindowContent();
        }
    }
}
