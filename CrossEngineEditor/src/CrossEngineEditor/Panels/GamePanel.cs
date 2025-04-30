using CrossEngine.Rendering.Cameras;
using CrossEngine.Scenes;
using CrossEngine.Core.Services;
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

        public GamePanel()
        {
            WindowName = "Game";
        }

        public override void OnAttach()
        {
            EditorApplication.Service.Context.SceneChanged += OnSceneChanged;
        }

        public override void OnDetach()
        {
            EditorApplication.Service.Context.SceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene old)
        {
            OnSurfaceResize(Surface, Surface.Size.X, Surface.Size.Y);
        }

        protected override void OnSurfaceResize(ISurface surface, float width, float height)
        {
            base.OnSurfaceResize(surface, width, height);
            
            Scene?.World.GetSystem<RenderSystem>().OnSurfaceResize(surface, width, height);
        }
    }
}
