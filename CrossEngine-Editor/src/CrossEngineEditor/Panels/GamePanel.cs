using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Layers;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Rendering;
using CrossEngine.Inputs;

namespace CrossEngineEditor.Panels
{
    class GamePanel : SceneViewPanel
    {
        Vector2 viewportSize;

        public GamePanel()
        {
            WindowName = "Game";
        }

        protected override void DrawWindowContent()
        {
            if (Context.Scene == null) return;

            //Drawing = !Context.Playmode;
            //if (Context.Playmode)
            //    SceneManager.Render();
            base.DrawWindowContent();

            if (!Context.Playmode)
                return;

            if (ViewportResized)
                Context.Scene.OnEvent(new WindowResizeEvent((uint)ViewportSize.X, (uint)ViewportSize.Y));
        }

        public override void OnEvent(Event e)
        {
            if (Context.Playmode)
            {
                if (Focused)
                {
                    Context.Scene.OnEvent(e);
                }
                else
                {
                    Input.ForceReset();
                }
            }
        }
    }
}
