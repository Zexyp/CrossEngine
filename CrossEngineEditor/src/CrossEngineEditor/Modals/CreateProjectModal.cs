using ImGuiNET;
using System;
using System.IO;

namespace CrossEngineEditor.Modals;

public class CreateProjectModal : EditorModal
{
    public CreateProjectModal() : base("Create Project")
    {
        Open = true;
        ModalFlags = ImGuiWindowFlags.AlwaysAutoResize;
    }
    
    string directory = Path.GetTempPath();
    protected override void DrawModalContent()
    {
        ImGui.InputText("Project Directory", ref directory, 260);
        if (ImGui.Button("Create"))
            throw new NotImplementedException();
    }
}