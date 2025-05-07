using System;
using System.Diagnostics;
using CrossEngineEditor.Utils;
using ImGuiNET;

namespace CrossEngineEditor.Modals;

public abstract class ImportModal : EditorModal
{
    protected ImportModal()
    {
        ModalName = this.GetType().Name;
    }
    
    protected override void DrawModalContent()
    {
        ImGui.Separator();
        
        if (ImGui.Button("Cancel"))
        {
            End();
            return;
        }
        ImGui.SameLine();
        if (ImGui.Button("Import"))
        {
            try
            {
                Process();
            }
            catch (Exception e)
            {
                EditorApplication.Service.DialogGenericError();
                EditorService.Log.Error($"importer failed ({this.GetType().FullName}):\n{e}");
            }
            End();
        }
    }
    
    protected abstract void Process();
}