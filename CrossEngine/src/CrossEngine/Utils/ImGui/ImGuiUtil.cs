using System;
using ImGuiNET;

using System.Numerics;
using System.Runtime.CompilerServices;

namespace CrossEngineEditor.Utils
{
    static class ImGuiUtil
    {
        // ❤️ https://github.com/ocornut/imgui/issues/2913
        // item spacing decides spacing
        public static void BeginPaddedGroup()
        {
            //Outer group
            ImGui.BeginGroup();

            ImGui.Dummy(new Vector2());
            ImGui.Dummy(new Vector2());
            ImGui.SameLine();
                
            //Inner group
            ImGui.BeginGroup();
        }

        public static void EndPaddedGroup()
        {
            //End inner group
            ImGui.EndGroup();

            ImGui.SameLine();
            ImGui.Dummy(new Vector2());
            ImGui.Dummy(new Vector2());

            //End outer group
            ImGui.EndGroup();
            
            var style = ImGui.GetStyle();
            ImGui.GetWindowDrawList().AddRect(
                ImGui.GetItemRectMin(),
                ImGui.GetItemRectMax(), 
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

        public static bool CenterButton(string label)
        {
            var style = ImGui.GetStyle();
            float size = ImGui.CalcTextSize(label).X + style.FramePadding.X * 2.0f;
            float avail = ImGui.GetContentRegionAvail().X;

            float off = (avail - size) * .5f;
            if (off > 0.0f)
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);

            return ImGui.Button(label);
        }
    }
}
