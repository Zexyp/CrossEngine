using System;
using ImGuiNET;

using System.Numerics;
using System.Runtime.CompilerServices;

using IG = ImGuiNET.ImGui;

namespace CrossEngine.Utils.ImGui
{
    public static class ImGuiUtil
    {
        // ❤️ https://github.com/ocornut/imgui/issues/2913
        // item spacing decides spacing
        public static void BeginPaddedGroup()
        {
            //Outer group
            IG.BeginGroup();

            IG.Dummy(new Vector2());
            IG.Dummy(new Vector2());
            IG.SameLine();
                
            //Inner group
            IG.BeginGroup();
        }

        public static void EndPaddedGroup()
        {
            //End inner group
            IG.EndGroup();

            IG.SameLine();
            IG.Dummy(new Vector2());
            IG.Dummy(new Vector2());

            //End outer group
            IG.EndGroup();
            
            var style = IG.GetStyle();
            IG.GetWindowDrawList().AddRect(
                IG.GetItemRectMin(),
                IG.GetItemRectMax(), 
                IG.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.Separator]), style.FrameRounding, ImDrawFlags.None, 1.5f);
        }

        // is not dumb and fills till end of column
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SmartSeparator(float thickness = 1.5f)
        {
            var colwidth = IG.GetColumnWidth();
            IG.Dummy(new Vector2(colwidth, thickness));
            Vector2 p = IG.GetCursorScreenPos();
            IG.GetWindowDrawList().AddLine(new Vector2(p.X, p.Y), new Vector2(p.X + colwidth, p.Y), IG.GetColorU32(ImGuiCol.Separator), thickness);
            IG.Dummy(new Vector2(colwidth, thickness));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SquareButton(string text)
        {
            var style = IG.GetStyle();
            var font = IG.GetFont();
            return IG.Button(text, new(style.FramePadding.Y * 2 + font.FontSize * font.Scale));
        }

        public static bool CenterButton(string label)
        {
            var style = IG.GetStyle();
            float size = IG.CalcTextSize(label).X + style.FramePadding.X * 2.0f;
            float avail = IG.GetContentRegionAvail().X;

            float off = (avail - size) * .5f;
            if (off > 0.0f)
                IG.SetCursorPosX(IG.GetCursorPosX() + off);

            return IG.Button(label);
        }

        public static bool FullWidthButton(string label)
        {
            return IG.Button(label, new(IG.GetColumnWidth(), 0.0f));
        }
    }
}
