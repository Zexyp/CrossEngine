using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Utils;

namespace CrossEngineEditor.Modals
{
    public abstract class EditorModal
    {
        public string ModalName = "";
        public bool? Open = true;
        public ImGuiWindowFlags ModalFlags = ImGuiWindowFlags.AlwaysAutoResize;

        public EditorModal(string name)
        {
            this.ModalName = name;
        }

        public bool Draw()
        {
            // hot fix
            ImGui.OpenPopup(ModalName);

            if (ImGuiExtension.BeginPopupModalNullableOpen(ModalName, ref Open, ModalFlags))
            {
                DrawContents();

                ImGui.EndPopup();
            }

            if ((Open != null) && !(bool)Open)
            {
                Default();
            }

            return ImGui.IsPopupOpen(ModalName);
        }

        protected abstract void DrawContents();
        protected virtual void Default() { }
    }
}
