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
        }

        protected override void AugmentSceneRender()
        {
            if (Scene?.World.GetSystem<RenderSystem>().PrimaryCamera != null)
                return;
            
            LineRenderer.BeginScene(Matrix4x4.Identity);
            LineRenderer.DrawLine(new Vector3(-1, -1, 0), new Vector3(1, 1, 0), VecColor.Red);
            LineRenderer.DrawLine(new Vector3(-1, 1, 0), new Vector3(1, -1, 0), VecColor.Red);
            LineRenderer.EndScene();
        }
    }
}
