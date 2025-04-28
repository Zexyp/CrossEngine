using System;
using ImGuiNET;

using System.Numerics;
using System.Runtime.CompilerServices;

namespace CrossEngineEditor.Utils
{
    static class ImGuiUtil
    {
        /*
        public static void BeginGroupFrame()
        {
            var style = ImGui.GetStyle();

            ImGui.BeginGroup();
            
            // spacing y
            ImGui.Dummy(new Vector2(0, style.FramePadding.Y));
            
            // spacing x
            if (style.FramePadding.X > 0) ImGui.Indent(style.FramePadding.X);
            
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(style.WindowPadding.X + style.FramePadding.X, style.WindowPadding.Y));
        }
        
        public static void EndGroupFrame()
        {
            ImGui.PopStyleVar();

            var style = ImGui.GetStyle();
            
            // spacing x
            if (style.FramePadding.X > 0) ImGui.Unindent();
            
            // spacing y
            ImGui.Dummy(new Vector2(0, style.FramePadding.Y));
            
            ImGui.EndGroup();

            ImGui.GetWindowDrawList().AddRect(
                ImGui.GetItemRectMin(),
                ImGui.GetItemRectMax() + new Vector2(style.FramePadding.X),
                ImGui.ColorConvertFloat4ToU32(style.Colors[(int)ImGuiCol.Separator]), style.FrameRounding, ImDrawFlags.None, 1.5f);
        }
        */
        
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
    }
}
