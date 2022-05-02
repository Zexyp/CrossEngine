using System;
using ImGuiNET;

using System.Numerics;
using System.Runtime.CompilerServices;

namespace CrossEngineEditor.Utils
{
    static class ImGuiUtils
    {
        public static void BeginGroupFrame()
        {
            var style = ImGui.GetStyle();

            ImGui.BeginGroup();
            ImGui.Dummy(new Vector2(0, style.FramePadding.Y));
            if (style.FramePadding.X > 0) ImGui.Indent(style.FramePadding.X);
        }
        public static void EndGroupFrame()
        {
            var style = ImGui.GetStyle();

            if (style.FramePadding.X > 0) ImGui.Unindent();
            ImGui.Dummy(new Vector2(0, style.FramePadding.Y));
            ImGui.EndGroup();
            ImGui.GetWindowDrawList().AddRect(
                ImGui.GetItemRectMin(),
                new Vector2(ImGui.GetColumnWidth() + style.FramePadding.X * 2 + ImGui.GetWindowPos().X, ImGui.GetItemRectMax().Y),
                ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.Separator]), style.FrameRounding, ImDrawFlags.None, 1.5f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SmartSeparator(float thickness = 1.5f)
        {
            var colwidth = ImGui.GetColumnWidth();
            ImGui.Dummy(new Vector2(colwidth, thickness));
            Vector2 p = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(new Vector2(p.X, p.Y), new Vector2(p.X + colwidth, p.Y), ImGui.GetColorU32(ImGuiCol.Separator), thickness);
            ImGui.Dummy(new Vector2(colwidth, thickness));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SquareButton(string text)
        {
            var style = ImGui.GetStyle();
            var font = ImGui.GetFont();
            return ImGui.Button(text, new(style.FramePadding.Y * 2 + font.FontSize * font.Scale));
        }
    }
}
