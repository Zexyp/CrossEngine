using System;
using System.Numerics;
using ImGuiNET;

namespace CrossEngineEditor.Modals;

public class ActionModal : EditorModal
{
    [Flags]
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
    private readonly ButtonFlags[] UniqueButtonFlags = {
        ButtonFlags.OK,
        ButtonFlags.Cancel,
        ButtonFlags.Yes,
        ButtonFlags.No,
    };

    public enum TextColor
    {
        None = 0,
        
        Info = None,
        Warn,
        Error,
    }

    public string Text;
    public Action<ButtonFlags> Action;
    public Action Success;
    public Action Failure;
    public ButtonFlags Buttons;
    public TextColor Color;


    public ActionModal(string text, string name = "", ButtonFlags buttons = ButtonFlags.OK, Action<ButtonFlags> action = null) : base(name)
    {
        this.Text = text;
        this.Action = action;
        this.Buttons = buttons;
    }

    protected override unsafe void DrawModalContent()
    {
        switch (Color)
        {
            case TextColor.Warn:
                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00ADE7);
                break;
            case TextColor.Error:
                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000E7);
                break;
        }

        ImGui.Text(Text);

        if (Color != TextColor.None)
            ImGui.PopStyleColor();

        ImGui.Separator();

        for (int i = 0; i < UniqueButtonFlags.Length; i++)
        {
            ref ButtonFlags flag = ref UniqueButtonFlags[i];
            if ((Buttons & flag) > 0)
            {
                if (ImGui.Button(flag.ToString(), new Vector2(60, 0)))
                {
                    SendEvents(flag);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
            }
        }

        if (!ImGui.IsWindowFocused())
            return;

        if (ImGui.IsKeyPressed(ImGuiKey.Escape))
        {
            ImGui.CloseCurrentPopup();
            Default();
        }

        //if (ImGui.IsKeyPressed(ImGuiKey.Enter))
        //{
        //    ImGui.CloseCurrentPopup();
        //    SendEvents(ButtonFlags.Yes);
        //}
    }

    protected override void Default()
    {
        SendEvents(ButtonFlags.None);
    }

    private void SendEvents(in ButtonFlags flag)
    {
        if (flag == ButtonFlags.Yes || flag == ButtonFlags.OK)
            Success?.Invoke();
        if (flag == ButtonFlags.No || flag == ButtonFlags.Cancel)
            Failure?.Invoke();
        Action?.Invoke(flag);
    }
}