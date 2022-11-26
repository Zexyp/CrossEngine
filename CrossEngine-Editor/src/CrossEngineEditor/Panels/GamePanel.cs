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
using CrossEngine.ComponentSystems;
using CrossEngine.Inputs;

namespace CrossEngineEditor.Panels
{
    class GamePanel : SceneViewPanel
    {
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

            if (Context.Scene.GetSystem<RendererSystem>().PrimaryCamera != null)
                base.DrawWindowContent();
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, .5f, .5f, 1));
                ImGui.Text("No primary camera");
                ImGui.PopStyleColor();
            }
        }

        protected override void Resized()
        {
            if (Context.Scene == null) return;

            if (!Context.Playmode)
                Context.Scene.GetSystem<RendererSystem>().Resize(ViewportSize.X, ViewportSize.Y);
            else
                OnEvent(new WindowResizeEvent((uint)ViewportSize.X, (uint)ViewportSize.Y));
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
