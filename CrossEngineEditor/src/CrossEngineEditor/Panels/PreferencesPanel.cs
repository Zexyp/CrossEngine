using CrossEngine.Utils;
using CrossEngineEditor.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Panels
{
    internal class PreferencesPanel : EditorPanel
    {
        public PreferencesPanel() : base("Preferences")
        {

        }

        protected unsafe override void DrawWindowContent()
        {
            if (ImGui.Button("Save")) EditorPreferences.Write(EditorApplication.Instance.Manager.GetService<EditorService>().Preferences, "preferences.json");
            ImGui.SameLine();
            if (ImGui.Button("Load")) EditorApplication.Instance.Manager.GetService<EditorService>().Preferences = EditorPreferences.Read("preferences.json");
            ImGui.SameLine();
            if (ImGui.Button("Load Defaults")) EditorApplication.Instance.Manager.GetService<EditorService>().Preferences = new EditorPreferences();
            ImGui.Separator();
            InspectDrawer.Inspect(EditorApplication.Instance.Manager.GetService<EditorService>().Preferences);
        }
    }
}
