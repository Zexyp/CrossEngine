using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Utils.UI
{
    internal abstract class Popup
    {
        public void Open()
        {
            ImGui.OpenPopup(this.GetHashCode().ToString());
        }

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
