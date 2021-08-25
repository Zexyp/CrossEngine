using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Utils;

namespace CrossEngineEditor
{
    public abstract class EditorModal
    {
        public string Name = "";
        public bool? Open = true;

        public EditorModal(string name)
        {
            this.Name = name;
        }

        public unsafe bool Draw()
        {
            ImGui.OpenPopup(Name);

            if (ImGuiExtension.BeginPopupModalNullableOpen(Name, ref Open, ImGuiWindowFlags.AlwaysAutoResize))
            {
                DrawContents();

                ImGui.EndPopup();
            }

            if ((Open != null) && !(bool)Open)
            {
                Default();
            }

            return ImGui.IsPopupOpen(Name);
        }

        protected abstract void DrawContents();
        protected virtual void Default() { }
    }
}
