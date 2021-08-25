using System;
using ImGuiNET;

using System.IO;
using System.Numerics;

using CrossEngine.Utils;

using CrossEngineEditor.Utils;

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
                    if (FileDialog.Open(out string path, initialDir: Environment.CurrentDirectory))
                    {
                        CrossEngine.Serialization.Json.JsonDeserializer deserializer = new CrossEngine.Serialization.Json.JsonDeserializer(CrossEngine.Serialization.Json.JsonSerialization.CreateBaseConvertersCollection());
                        EditorLayer.Instance.Scene = (CrossEngine.Scenes.Scene)deserializer.Deserialize(File.ReadAllText(path), typeof(CrossEngine.Scenes.Scene));
                    }

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
