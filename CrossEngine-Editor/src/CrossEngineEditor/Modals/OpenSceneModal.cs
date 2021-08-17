using ImGuiNET;

using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngineEditor
{
    class OpenSceneModal : EditorModal
    {
        public OpenSceneModal() : base(nameof(OpenSceneModal)) { }

        protected unsafe override void DrawContents()
        {
            if (ImGuiExtension.BeginPopupModalNullableOpen(Name, null, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("All those beautiful changes will be lost.\nThis operation cannot be undone!\n");

                ImGui.Separator();

                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    //OpenFileDialog dialog

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
