using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Utils;

using CrossEngineEditor.Panels;
using CrossEngineEditor.Modals;
using CrossEngineEditor.Utils;
using CrossEngineEditor.Utils.Gui;

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
                        if (!ImGuiStyleConfig.Load(new IniFile("style"))) EditorLayer.Instance.PushModal(new ActionModal("Config seems to be corrupted!", "Ouha!", ActionModal.ButtonFlags.OK));
                    }

                    if (ImGui.Button("Save"))
                    {
                        ImGuiStyleConfig.Save(new IniFile("style"));
                    }

                    ImGuiUtils.SmartSeparator();

                    if (ImGui.Button("Load From File..."))
                    {
                        if (Dialog.FileOpen(out string path,
                            filters: new[]{
                                Dialog.Filters.IniFile,
                                Dialog.Filters.AllFiles }))
                        {
                            if (!ImGuiStyleConfig.Load(new IniFile(path, true))) EditorLayer.Instance.PushModal(new ActionModal("Config seems to be corrupted!", "Ouha!", ActionModal.ButtonFlags.OK));
                        }
                    }

                    ImGuiUtils.EndGroupFrame();

                    ImGui.EndChild();
                }
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
            }
        }
    }
}
