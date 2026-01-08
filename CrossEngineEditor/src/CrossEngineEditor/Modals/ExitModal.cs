using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Modals
{
    class ExitModal : EditorModal
    {
        public ExitModal() : base("Confirm Exit")
        {
            
        }

        public Action Exit;

        protected override void DrawModalContent()
        {
            ImGui.Text("Are you sure you want to exit?");
            
            ImGui.Separator();

            if (ImGui.Button("Exit"))
            {
                Exit.Invoke();
                End();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
                End();

            if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                End();
            if (ImGui.IsKeyPressed(ImGuiKey.Enter))
            {
                Exit.Invoke();
                End();
            }
        }
    }
}
