using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor
{
    abstract class EditorModal
    {
        public string Name = "";
        public EditorModal(string name)
        {
            this.Name = name;
        }

        public bool Draw()
        {
            ImGui.OpenPopup(Name);
            DrawContents();
            return ImGui.IsPopupOpen(Name);
        }

        protected abstract void DrawContents();
    }
}
