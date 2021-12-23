using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngineEditor.Panels;
using CrossEngineEditor.Modals;
using CrossEngineEditor.Utils;

namespace CrossEngineEditor.Panels
{
    class ConfigPanel : EditorPanel
    {
        public ConfigPanel() : base("Config")
        {
            Open = false;
        }

        protected override void DrawWindowContent()
        {
            // left
            {
                Vector2 childSize = ImGui.GetContentRegionAvail();
                childSize.X *= 0.2f;
                ImGui.BeginChild("##left", childSize);
                {
                    ImGuiUtils.BeginGroupFrame();

                    if (ImGui.Button("Load"))
                    {
                        ImGuiStyleConfig.Load(new IniConfig("style"));
                    }

                    ImGuiUtils.SmartSeparator();

                    if (ImGui.Button("Save"))
                    {
                        ImGuiStyleConfig.Save(new IniConfig("style"));
                    }

                    ImGuiUtils.EndGroupFrame();

                }
                ImGui.EndChild();
            }

            ImGui.SameLine();

            // right
            {
                Vector2 childSize = ImGui.GetContentRegionAvail();
                ImGui.BeginChild("##right", childSize);
                {
                    ImGui.ShowStyleEditor();

                    ImGui.EndChild();
                }
                ImGui.EndChild();
            }
        }
    }
}
