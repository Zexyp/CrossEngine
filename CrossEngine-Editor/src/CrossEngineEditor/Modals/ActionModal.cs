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
        public enum ButtonFlags
        {
            None   = 0,
            OK     = 1 << 0,
            Cancel = 1 << 1,
            Yes    = 1 << 2,
            No     = 1 << 3,

            OKCancel = OK | Cancel,
            YesNo = Yes | No,
        }

        public enum TextColor
        {
            None = 0,
            
            Info = None,
            Warn,
            Error,
        }

        public string Text;
        public Action<ButtonFlags> Action;
        public ButtonFlags Buttons;
        public TextColor Color;

        public ActionModal(string text, string name = "", ButtonFlags buttons = ButtonFlags.None, Action <ButtonFlags> action = null) : base(name)
        {
            this.Text = text;
            this.Action = action;
            this.Buttons = buttons;
        }

        protected override unsafe void DrawContents()
        {
            switch (Color)
            {
                case TextColor.Warn: ImGui.PushStyleColor(ImGuiCol.Text, 0xE7AD00FF);
                    break;
                case TextColor.Error: ImGui.PushStyleColor(ImGuiCol.Text, 0xE70000FF);
                    break;
            }

            ImGui.Text(Text);

            if (Color != TextColor.None)
                ImGui.PopStyleColor();

            ImGui.Separator();

            if ((Buttons & ButtonFlags.OK) > 0)
            {
                if (ImGui.Button("OK", new Vector2(60, 0)))
                {
                    Action?.Invoke(ButtonFlags.OK);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ButtonFlags.Cancel) > 0)
            {
                if (ImGui.Button("Cancel", new Vector2(60, 0)))
                {
                    Action?.Invoke(ButtonFlags.Cancel);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ButtonFlags.Yes) > 0)
            {
                if (ImGui.Button("Yes", new Vector2(60, 0)))
                {
                    Action?.Invoke(ButtonFlags.Yes);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
            if ((Buttons & ButtonFlags.No) > 0)
            {
                if (ImGui.Button("No", new Vector2(60, 0)))
                {
                    Action?.Invoke(ButtonFlags.No);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
        }

        protected override void Default()
        {
            Action?.Invoke(ButtonFlags.None);
        }
    }
}
