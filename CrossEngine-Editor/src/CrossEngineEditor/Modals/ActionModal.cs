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
    public enum ActionModalButtonFlags
    {
        None = 0,
        OK = 1 << 0,
        Cancel = 1 << 1,
        Yes = 1 << 2,
        No = 1 << 3,
            
        OKCancel = OK | Cancel,
        YesNo = Yes | No,
    }

    public class ActionModal : EditorModal
    {

        string Text;
        Action<ActionModalButtonFlags> Action;
        ActionModalButtonFlags Buttons;

        public ActionModal(string text, ActionModalButtonFlags buttons = ActionModalButtonFlags.None, Action <ActionModalButtonFlags> action = null, string name = "") : base(name)
        {
            this.Text = text;
            this.Action = action;
            this.Buttons = buttons;
        }

        protected override unsafe void DrawContents()
        {
            ImGui.Text(Text);

            ImGui.Separator();

            if ((Buttons & ActionModalButtonFlags.OK) > 0)
            {
                if (ImGui.Button("OK", new Vector2(60, 0)))
                {
                    Action?.Invoke(ActionModalButtonFlags.OK);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ActionModalButtonFlags.Cancel) > 0)
            {
                if (ImGui.Button("Cancel", new Vector2(60, 0)))
                {
                    Action?.Invoke(ActionModalButtonFlags.Cancel);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ActionModalButtonFlags.Yes) > 0)
            {
                if (ImGui.Button("Yes", new Vector2(60, 0)))
                {
                    Action?.Invoke(ActionModalButtonFlags.Yes);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ActionModalButtonFlags.No) > 0)
            {
                if (ImGui.Button("No", new Vector2(60, 0)))
                {
                    Action?.Invoke(ActionModalButtonFlags.No);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
        }

        protected override void Default()
        {
            Action?.Invoke(ActionModalButtonFlags.None);
        }
    }
}
