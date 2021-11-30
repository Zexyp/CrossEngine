using System;
using ImGuiNET;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngineEditor.Modals
{
    public class ActionModal : EditorModal
    {
        public enum ModalButtonFlags
        {
            None = 0,
            OK = 1 << 0,
            Cancel = 1 << 1,
            Yes = 1 << 2,
            No = 1 << 3,
            
            OKCancel = OK | Cancel,
            YesNo = Yes | No,
        }

        string Text;
        Action<ModalButtonFlags> Action;
        ModalButtonFlags Buttons;

        public ActionModal(string text, ModalButtonFlags buttons = ModalButtonFlags.None, Action <ModalButtonFlags> action = null, string name = "") : base(name)
        {
            this.Text = text;
            this.Action = action;
            this.Buttons = buttons;
        }

        protected override unsafe void DrawContents()
        {
            ImGui.Text(Text);

            ImGui.Separator();

            if ((Buttons & ModalButtonFlags.OK) > 0)
            {
                if (ImGui.Button("OK", new Vector2(60, 0)))
                {
                    Action?.Invoke(ModalButtonFlags.OK);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ModalButtonFlags.Cancel) > 0)
            {
                if (ImGui.Button("Cancel", new Vector2(60, 0)))
                {
                    Action?.Invoke(ModalButtonFlags.Cancel);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ModalButtonFlags.Yes) > 0)
            {
                if (ImGui.Button("Yes", new Vector2(60, 0)))
                {
                    Action?.Invoke(ModalButtonFlags.Yes);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ModalButtonFlags.No) > 0)
            {
                if (ImGui.Button("No", new Vector2(60, 0)))
                {
                    Action?.Invoke(ModalButtonFlags.No);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
        }

        protected override void Default()
        {
            Action?.Invoke(ModalButtonFlags.None);
        }
    }
}
