using System.Diagnostics;
using CrossEngineEditor.Utils;
using ImGuiNET;

namespace CrossEngineEditor.Modals;

public abstract class ImportModal : EditorModal
{
    protected override void DrawModalContent()
    {
        ImGui.Separator();
        
        if (ImGui.Button("Cancel"))
        {
            End();
            return;
        }
        
        if (ImGui.Button("Import"))
        {
            Process();
            End();
        }
    }
    
    protected abstract void Process();
}