using CrossEngine.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngineEditor.Panels
{
    internal class SimpleThemeGeneratorPanel : EditorPanel
    {
        Vector4 @base = new(0.502f, 0.075f, 0.256f, 1.0f);
        Vector4 bg = new(0.200f, 0.220f, 0.270f, 1.0f);
        Vector4 text = new(0.860f, 0.930f, 0.890f, 1.0f);
        float high_val = 0.661f;
        float mid_val = 0.5f;
        float low_val = 0.3f;
        float window_offset = -0.2f;
        bool enabled = false;

        public SimpleThemeGeneratorPanel() : base("Theme Generator")
        {
            
        }

        protected unsafe override void DrawWindowContent()
        {
            ImGui.Checkbox("enabled", ref enabled);

            if (!enabled) ImGui.BeginDisabled();

            Vector4 make_high(float alpha) {
                Vector4 res = new(0, 0, 0, alpha);
                ImGui.ColorConvertRGBtoHSV(@base.X, @base.Y, @base.Z, out res.X, out res.Y, out res.Z);
                res.Z = high_val;
                ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
                return res;
            }

            Vector4 make_mid(float alpha) {
                Vector4 res = new(0, 0, 0, alpha);
                ImGui.ColorConvertRGBtoHSV(@base.X, @base.Y, @base.Z, out res.X, out res.Y, out res.Z);
                res.Z = mid_val;
                ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
                return res;
            }

            Vector4 make_low(float alpha) {
                Vector4 res = new(0, 0, 0, alpha);
                ImGui.ColorConvertRGBtoHSV(@base.X, @base.Y, @base.Z, out res.X, out res.Y, out res.Z);
                res.Z = low_val;
                ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
                return res;
            }

            Vector4 make_bg(float alpha, float offset = 0.0f) {
                Vector4 res = new(0, 0, 0, alpha);
                ImGui.ColorConvertRGBtoHSV(bg.X, bg.Y, bg.Z, out res.X, out res.Y, out res.Z);
                res.Z += offset;
                ImGui.ColorConvertHSVtoRGB(res.X, res.Y, res.Z, out res.X, out res.Y, out res.Z);
                return res;
            }

            Vector4 make_text(float alpha) {
                return new Vector4(text.X, text.Y, text.Z, alpha);
            }

            fixed (void* p = &@base)
                ImGui.ColorEdit3("base", ref Unsafe.AsRef<Vector3>(p), ImGuiColorEditFlags.PickerHueWheel);
            fixed (void* p = &bg)
                ImGui.ColorEdit3("bg", ref Unsafe.AsRef<Vector3>(p), ImGuiColorEditFlags.PickerHueWheel);
            fixed (void* p = &text)
                ImGui.ColorEdit3("text", ref Unsafe.AsRef<Vector3>(p), ImGuiColorEditFlags.PickerHueWheel);
            ImGui.SliderFloat("high", ref high_val, 0, 1);
            ImGui.SliderFloat("mid", ref mid_val, 0, 1);
            ImGui.SliderFloat("low", ref low_val, 0, 1);
            ImGui.SliderFloat("window", ref window_offset, -0.4f, 0.4f);

            if (!enabled) ImGui.EndDisabled();

            if (enabled)
            {
                var colors = ImGui.GetStyle().Colors;

                colors[(int)ImGuiCol.Text] = make_text(0.78f);
                colors[(int)ImGuiCol.TextDisabled] = make_text(0.28f);
                colors[(int)ImGuiCol.WindowBg] = make_bg(1.00f, window_offset);
                colors[(int)ImGuiCol.ChildBg] = make_bg(0.58f);
                colors[(int)ImGuiCol.PopupBg] = make_bg(0.9f);
                colors[(int)ImGuiCol.Border] = make_bg(0.6f, -0.05f);
                colors[(int)ImGuiCol.BorderShadow] = make_bg(0.0f, 0.0f);
                colors[(int)ImGuiCol.FrameBg] = make_bg(1.00f);
                colors[(int)ImGuiCol.FrameBgHovered] = make_mid(0.78f);
                colors[(int)ImGuiCol.FrameBgActive] = make_mid(1.00f);
                colors[(int)ImGuiCol.TitleBg] = make_bg(1.00f, -.1f); //make_low(1.00f);
                colors[(int)ImGuiCol.TitleBgActive] = make_low(1.00f); //make_high(1.00f);
                colors[(int)ImGuiCol.TitleBgCollapsed] = make_bg(0.75f);
                colors[(int)ImGuiCol.MenuBarBg] = make_bg(0.47f);
                colors[(int)ImGuiCol.ScrollbarBg] = make_bg(1.00f);
                colors[(int)ImGuiCol.ScrollbarGrab] = make_low(1.00f);
                colors[(int)ImGuiCol.ScrollbarGrabHovered] = make_mid(0.78f);
                colors[(int)ImGuiCol.ScrollbarGrabActive] = make_mid(1.00f);
                colors[(int)ImGuiCol.CheckMark] = make_high(1.00f);
                colors[(int)ImGuiCol.SliderGrab] = make_bg(1.0f, .1f);
                colors[(int)ImGuiCol.SliderGrabActive] = make_high(1.0f);
                colors[(int)ImGuiCol.Button] = make_bg(1.0f, .2f);
                colors[(int)ImGuiCol.ButtonHovered] = make_mid(1.00f);
                colors[(int)ImGuiCol.ButtonActive] = make_high(1.00f);
                colors[(int)ImGuiCol.Header] = make_mid(0.76f);
                colors[(int)ImGuiCol.HeaderHovered] = make_mid(0.86f);
                colors[(int)ImGuiCol.HeaderActive] = make_high(1.00f);
                colors[(int)ImGuiCol.ResizeGrip] = new(0.47f, 0.77f, 0.83f, 0.04f);
                colors[(int)ImGuiCol.ResizeGripHovered] = make_mid(0.78f);
                colors[(int)ImGuiCol.ResizeGripActive] = make_mid(1.00f);
                colors[(int)ImGuiCol.PlotLines] = make_text(0.63f);
                colors[(int)ImGuiCol.PlotLinesHovered] = make_mid(1.00f);
                colors[(int)ImGuiCol.PlotHistogram] = make_text(0.63f);
                colors[(int)ImGuiCol.PlotHistogramHovered] = make_mid(1.00f);
                colors[(int)ImGuiCol.TextSelectedBg] = make_mid(0.43f);
                colors[(int)ImGuiCol.ModalWindowDimBg] = make_bg(0.73f);
                colors[(int)ImGuiCol.Tab] = make_bg(0.40f);
                colors[(int)ImGuiCol.TabHovered] = make_high(1.00f);
                colors[(int)ImGuiCol.TabActive] = make_mid(1.00f);
                colors[(int)ImGuiCol.TabUnfocused] = make_bg(0.40f);
                colors[(int)ImGuiCol.TabUnfocusedActive] = make_bg(0.70f);
                colors[(int)ImGuiCol.DockingPreview] = make_high(0.30f);
                colors[(int)ImGuiCol.SeparatorHovered] = make_mid(0.78f);
                colors[(int)ImGuiCol.SeparatorActive] = make_mid(1.00f);
            }

            if (ImGui.Button("Export"))
            {
                ImGui.LogToTTY();
                ImGui.LogText("var colors = ImGui.GetStyle().Colors;\n");

                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";

                var colors = ImGui.GetStyle().Colors;

                for (int i = 0; i < (int)ImGuiCol.COUNT; i++)
                {
                    var col = colors[i];
                    var name = ImGui.GetStyleColorName((ImGuiCol)i);
                    ImGui.LogText($"colors[(int)ImGuiCol.{name}] = new({col.X.ToString(nfi)}f, {col.Y.ToString(nfi)}f, {col.Z.ToString(nfi)}f, {col.W.ToString(nfi)}f);\n");
                }
                ImGui.LogFinish();
            }
        }
    }
}
