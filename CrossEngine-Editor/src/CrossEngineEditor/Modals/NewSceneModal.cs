using System;
using ImGuiNET;

using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngineEditor
{
    class NewSceneModal : EditorModal
    {
        public NewSceneModal() : base(nameof(NewSceneModal)) { }

        protected unsafe override void DrawContents()
        {
            if (ImGuiExtension.BeginPopupModalNullableOpen(Name, null, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("All those beautiful changes will be lost.\nThis operation cannot be undone!\n");

                ImGui.Separator();
                
                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    EditorLayer.Instance.Scene = new CrossEngine.Scenes.Scene();
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
        }
    }
}
