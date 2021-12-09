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

        public static void EndGroupFrame(uint color, float rounding = 5)
        {
            var style = ImGui.GetStyle();

            if (style.FramePadding.X > 0) ImGui.Unindent();
            ImGui.Dummy(new Vector2(0, style.FramePadding.Y));
            ImGui.EndGroup();
            ImGui.GetWindowDrawList().AddRect(
                ImGui.GetItemRectMin(),
                new Vector2(ImGui.GetColumnWidth() + style.FramePadding.X, ImGui.GetItemRectMax().Y),
                color, rounding);
        }
    }
}
