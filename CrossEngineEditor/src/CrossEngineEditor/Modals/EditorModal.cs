using CrossEngine.Utils.ImGui;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Modals
{
    internal abstract class EditorModal
    {
        public string PopupName;
        //public bool? Open = true;

        public EditorModal(string name)
        {
            this.PopupName = name;
        }

        public EditorModal()
        {
            this.PopupName = $"Unnamed Popup '{this.GetType().FullName}'";
        }

        public void Draw()
        {
            if (ImGui.BeginPopupModal(PopupName))
            {
                DrawModalContent();

                ImGui.EndPopup();
            }
        }

        abstract protected void DrawModalContent();
    }
}
