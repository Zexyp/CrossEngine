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
        public void Draw()
        {
            if (ImGui.BeginPopup(this.GetHashCode().ToString()))
            {
                DrawPopupContent();

                ImGui.EndPopup();
            }
        }
        abstract protected void DrawPopupContent();
    }
}
