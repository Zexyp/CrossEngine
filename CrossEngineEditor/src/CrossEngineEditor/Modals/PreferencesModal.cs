using System;
using System.Collections.Generic;
using System.IO;
using CrossEngine.Utils;
using ImGuiNET;

namespace CrossEngineEditor.Modals;

class PreferencesModal : EditorModal
{
    public PreferencesModal() : base("Preferences")
    {
        ModalFlags = ImGuiWindowFlags.None;
    }
    
    protected override void DrawModalContent()
    {
        var val = EditorApplication.Service.Preferences["navigation"].ReadBooleanOrDefault("touchpad", false);
        if (ImGui.Checkbox("Touchpad", ref val))
            EditorApplication.Service.Preferences["navigation"].Write("touchpad", val);

        ImGui.Separator();
        
        if (ImGui.Button("Save"))
            IniFile.Dump(EditorApplication.Service.Preferences, File.Create(EditorService.PreferencesPath));
        ImGui.SameLine();
        if (ImGui.Button("Close"))
            End();
    }
}