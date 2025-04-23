using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossEngineEditor.Modals
{
    class BlockModal : EditorModal
    {
        public string Text = "";

        public BlockModal(string name) : base(name)
        {
            Open = null;
        }

        protected override void DrawModalContent()
        {
            ImGui.Text(Text);
            //ImGui.Text("Please close the file dialog to continue.");
            
            //if (ImGui.Button("I don't care"))
            //    EditorApplication.Service.DestructiveDialog(() => this.Open = false);
        }
    }
}
