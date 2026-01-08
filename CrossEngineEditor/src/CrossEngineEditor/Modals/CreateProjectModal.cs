using ImGuiNET;
using System;
using System.IO;

namespace CrossEngineEditor.Modals;

class CreateProjectModal : EditorModal
{
    public CreateProjectModal() : base("Create Project")
    {
        Open = true;
        ModalFlags = ImGuiWindowFlags.AlwaysAutoResize;
    }
    
    string directory = Path.GetTempPath();
    string name = "NewProject";
    public Action<string> Callback;
    protected override void DrawModalContent()
    {
        ImGui.TextDisabled("please sanitize the paths, i'm not paid enough");
        ImGui.InputText("Directory", ref directory, 260);
        ImGui.SameLine();
        if (ImGui.Button("..."))
            EditorApplication.Service.DialogPickDirectory().ContinueWith(t => { var dirname = t.Result; if (dirname != null) directory = dirname; });

        ImGui.InputText("Name", ref name, 128);
        var projDir = Path.Join(directory, name);
        ImGui.TextDisabled($"Project directory: {projDir}");

        ImGui.Separator();
        if (ImGui.Button("Create"))
        {
            Callback.Invoke(Path.Join(projDir, $"{name}.ceproj"));
            End();
        }
    }
}