using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Popups
{
    internal abstract class EditorPopup
    {
        public string Title;

        public EditorPopup(string title)
        {
            this.Title = title;
        }

        public EditorPopup()
        {
            this.Title = $"Unnamed Popup ({this.GetType().FullName})";
        }

        public void Draw()
        {
            if (ImGui.BeginPopup(Title))
            {
                DrawPopupContent();

                ImGui.EndPopup();
            }
        }
        abstract protected void DrawPopupContent();
    }
}
