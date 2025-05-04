using System;
using System.Collections.Generic;
using System.IO;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.IO;
using CrossEngineEditor.Platform;
using CrossEngineEditor.Utils;
using ImGuiNET;

namespace CrossEngineEditor.Modals;

class PreferencesModal : EditorModal
{
    public struct Navigation
    {
        [EditorValue]
        public bool Toucpad
        {
            get => EditorApplication.Service.Preferences["navigation"].ReadBooleanOrDefault("touchpad", false);
            set => EditorApplication.Service.Preferences["navigation"].Write("touchpad", value);
        }
        [EditorValue]
        public float ToucpadSensitivityPan
        {
            get => EditorApplication.Service.Preferences["navigation"].ReadSingleOrDefault("touchpad.sensitivity.pan", .25f);
            set => EditorApplication.Service.Preferences["navigation"].Write("touchpad.sensitivity.pan", value);
        }
        [EditorValue]
        public float ToucpadSensitivityRotate
        {
            get => EditorApplication.Service.Preferences["navigation"].ReadSingleOrDefault("touchpad.sensitivity.rotate", 8);
            set => EditorApplication.Service.Preferences["navigation"].Write("touchpad.sensitivity.rotate", value);
        }
    }
    
    public PreferencesModal() : base("Preferences")
    {
        ModalFlags = ImGuiWindowFlags.None;
    }

    [EditorInnerDraw]
    public Navigation Nav;
    
    protected override void DrawModalContent()
    {
        //var val = EditorApplication.Service.Preferences["navigation"].ReadBooleanOrDefault("touchpad", false);
        //if (ImGui.Checkbox("Touchpad", ref val))
        //    EditorApplication.Service.Preferences["navigation"].Write("touchpad", val);
        
        InspectDrawer.Inspect(this);

        ImGui.Separator();
        
        if (ImGui.Button("Save"))
            IniFile.Dump(EditorApplication.Service.Preferences, EditorPlatformHelper.FileCreate(EditorService.ConfigPreferencesPath));
        ImGui.SameLine();
        if (ImGui.Button("Close"))
            End();
    }
}