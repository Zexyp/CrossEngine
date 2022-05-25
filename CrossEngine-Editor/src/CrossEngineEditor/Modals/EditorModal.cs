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
        public bool Pushed = false;

        public EditorModal(string name)
        {
            this.ModalName = name;
        }

        /// <summary>
        /// Draws the modal and returns <see langword="false"/> if modal was closed.
        /// </summary>
        /// <returns></returns>
        public bool Draw()
        {
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
