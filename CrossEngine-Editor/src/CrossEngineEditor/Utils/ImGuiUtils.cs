using System;
using ImGuiNET;

using System.Numerics;

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
                new Vector2(ImGui.GetColumnWidth() + style.FramePadding.X + ImGui.GetWindowPos().X, ImGui.GetItemRectMax().Y),
                ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.Separator]), style.FrameRounding, ImDrawFlags.None, 1.5f);
        }

        public static void SmartSeparator(float thickness = 1.5f)
        {
            Vector2 p = ImGui.GetCursorScreenPos();
            ImGui.GetWindowDrawList().AddLine(new Vector2(p.X, p.Y), new Vector2(p.X + ImGui.GetColumnWidth(), p.Y), ImGui.GetColorU32(ImGuiCol.Separator), thickness);
            ImGui.Dummy(new Vector2(ImGui.GetColumnWidth(), thickness));
        }
    }
}
