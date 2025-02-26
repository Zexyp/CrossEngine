using CrossEngine.Platform.Glfw;
using CrossEngine.Services;
using System;

using ImGuiNET;
using IG = ImGuiNET.ImGui;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.IO;
using CrossEngine.Logging;
using CrossEngine.Core;
using CrossEngine.Display;

namespace CrossEngine.Utils.ImGui
{
    public class ImGuiService : Service
    {
        MyImGuiController controller;

        private string fontpath = null;
        private Logger _log = new Logger("imgui-service");

        public ImGuiService(string fontpath = null)
        {
            this.fontpath = fontpath;
        }

        public override void OnDetach()
        {
            _log.Trace("detaching");

            var rs = Manager.GetService<RenderService>();
            rs.BeforeDraw -= OnBeforeDraw;
            rs.AfterDraw -= OnAfterDraw;

            rs.Execute(() =>
            {
                // TODO: dispose
            });
        }

        public override unsafe void OnAttach()
        {
            _log.Trace("attaching");
            
            var rs = Manager.GetService<RenderService>();
            rs.BeforeDraw += OnBeforeDraw;
            rs.AfterDraw += OnAfterDraw;

            rs.Execute(() =>
            {
                controller = new MyImGuiController(
                    CrossEngine.Platform.OpenGL.GLContext.gl,
                    Manager.GetService<WindowService>().MainWindow,
                    () =>
                    {
                        var io = IG.GetIO();
                        
                        if (fontpath != null) io.Fonts.AddFontFromFileTTF(fontpath, 16f);

                        SetupTheme();

                        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;    // Enable Docking
                    });
            });
        }

        private void OnBeforeDraw(RenderService rs)
        {
            controller.Update(Time.UnscaledDeltaF);
        }

        private unsafe void OnAfterDraw(RenderService rs)
        {
            var w = Manager.GetService<WindowService>().MainWindow;
            rs.RendererApi.SetViewport(0, 0, w.Width, w.Height);
            controller.Render();
        }

        public override void OnStart()
        {
            
        }

        public override void OnDestroy()
        {
            
        }

        private void SetupTheme()
        {
            //IG.StyleColorsLight();
            var style = IG.GetStyle();
            var colors = style.Colors;

            //colors[(int)ImGuiCol.SliderGrab] = new(0.24f, 0.90f, 0.66f, 0.78f);
            // #3EE6A9, #50112C

            if (fontpath != null)
                colors[(int)ImGuiCol.Text] = new(0.86f, 0.93f, 0.89f, 1);
            else
                colors[(int)ImGuiCol.Text] = new(0.86f, 0.93f, 0.89f, 1);
            colors[(int)ImGuiCol.TextDisabled] = new(0.86f, 0.93f, 0.89f, 0.28f);
            colors[(int)ImGuiCol.WindowBg] = new(0.051851857f, 0.057037048f, 0.07000001f, .5f); //1f
            colors[(int)ImGuiCol.ChildBg] = new(0.2f, 0.22000003f, 0.27f, 0.58f);
            colors[(int)ImGuiCol.PopupBg] = new(0.2f, 0.22000003f, 0.27f, 0.9f);
            colors[(int)ImGuiCol.Border] = new(0.16296297f, 0.17925929f, 0.22000001f, 0.6f);
            colors[(int)ImGuiCol.BorderShadow] = new(0.2f, 0.22000003f, 0.27f, 0f);
            colors[(int)ImGuiCol.FrameBg] = new(0.2f, 0.22000003f, 0.27f, 1f);
            colors[(int)ImGuiCol.FrameBgHovered] = new(0.5f, 0.07470119f, 0.25498015f, 0.78f);
            colors[(int)ImGuiCol.FrameBgActive] = new(0.5f, 0.07470119f, 0.25498015f, 1f);
            colors[(int)ImGuiCol.TitleBg] = new(0.12592593f, 0.13851854f, 0.17000002f, 1f);
            colors[(int)ImGuiCol.TitleBgActive] = new(0.3f, 0.044820715f, 0.15298809f, 1f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new(0.2f, 0.22000003f, 0.27f, 0.75f);
            colors[(int)ImGuiCol.MenuBarBg] = new(0.2f, 0.22000003f, 0.27f, 0.47f);
            colors[(int)ImGuiCol.ScrollbarBg] = new(0.2f, 0.22000003f, 0.27f, 1f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new(0.3f, 0.044820715f, 0.15298809f, 1f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new(0.5f, 0.07470119f, 0.25498015f, 0.78f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new(0.5f, 0.07470119f, 0.25498015f, 1f);
            colors[(int)ImGuiCol.CheckMark] = new(0.661f, 0.09875497f, 0.33708376f, 1f);
            colors[(int)ImGuiCol.SliderGrab] = new(0.27407408f, 0.30148152f, 0.37f, 1f);
            colors[(int)ImGuiCol.SliderGrabActive] = new(0.661f, 0.09875497f, 0.33708376f, 1f);
            colors[(int)ImGuiCol.Button] = new(0.34814817f, 0.382963f, 0.47000003f, 1f);
            colors[(int)ImGuiCol.ButtonHovered] = new(0.5f, 0.07470119f, 0.25498015f, 1f);
            colors[(int)ImGuiCol.ButtonActive] = new(0.661f, 0.09875497f, 0.33708376f, 1f);
            colors[(int)ImGuiCol.Header] = new(0.5f, 0.07470119f, 0.25498015f, 0.76f);
            colors[(int)ImGuiCol.HeaderHovered] = new(0.5f, 0.07470119f, 0.25498015f, 0.86f);
            colors[(int)ImGuiCol.HeaderActive] = new(0.661f, 0.09875497f, 0.33708376f, 1f);
            colors[(int)ImGuiCol.Separator] = new(0.43f, 0.43f, 0.5f, 0.5f);
            colors[(int)ImGuiCol.ResizeGrip] = new(0.47f, 0.77f, 0.83f, 0.04f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new(0.5f, 0.07470119f, 0.25498015f, 0.78f);
            colors[(int)ImGuiCol.ResizeGripActive] = new(0.5f, 0.07470119f, 0.25498015f, 1f);
            colors[(int)ImGuiCol.Tab] = new(0.2f, 0.22000003f, 0.27f, 0.4f);
            colors[(int)ImGuiCol.TabHovered] = new(0.661f, 0.09875497f, 0.33708376f, 1f);
            colors[(int)ImGuiCol.TabActive] = new(0.5f, 0.07470119f, 0.25498015f, 1f);
            colors[(int)ImGuiCol.TabUnfocused] = new(0.2f, 0.22000003f, 0.27f, 0.4f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new(0.2f, 0.22000003f, 0.27f, 0.7f);
            colors[(int)ImGuiCol.DockingPreview] = new(0.661f, 0.09875497f, 0.33708376f, 0.3f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new(0.2f, 0.2f, 0.2f, 1f);
            colors[(int)ImGuiCol.PlotLines] = new(0.86f, 0.93f, 0.89f, 0.63f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new(0.5f, 0.07470119f, 0.25498015f, 1f);
            colors[(int)ImGuiCol.PlotHistogram] = new(0.86f, 0.93f, 0.89f, 0.63f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new(0.5f, 0.07470119f, 0.25498015f, 1f);
            colors[(int)ImGuiCol.TableHeaderBg] = new(0.19f, 0.19f, 0.2f, 1f);
            colors[(int)ImGuiCol.TableBorderStrong] = new(0.31f, 0.31f, 0.35f, 1f);
            colors[(int)ImGuiCol.TableBorderLight] = new(0.23f, 0.23f, 0.25f, 1f);
            colors[(int)ImGuiCol.TableRowBg] = new(0f, 0f, 0f, 0f);
            colors[(int)ImGuiCol.TableRowBgAlt] = new(1f, 1f, 1f, 0.06f);
            colors[(int)ImGuiCol.TextSelectedBg] = new(0.5f, 0.07470119f, 0.25498015f, 0.43f);
            colors[(int)ImGuiCol.DragDropTarget] = new(1f, 1f, 0f, 0.9f);
            colors[(int)ImGuiCol.NavHighlight] = new(0.26f, 0.59f, 0.98f, 1f);
            colors[(int)ImGuiCol.NavWindowingHighlight] = new(1f, 1f, 1f, 0.7f);
            colors[(int)ImGuiCol.NavWindowingDimBg] = new(0.8f, 0.8f, 0.8f, 0.2f);
            colors[(int)ImGuiCol.ModalWindowDimBg] = new(0.2f, 0.22000003f, 0.27f, 0.73f);
            colors[(int)ImGuiCol.SeparatorHovered] = new(0.5f, 0.07470119f, 0.25498015f, 0.78f);
            colors[(int)ImGuiCol.SeparatorActive] = new(0.5f, 0.07470119f, 0.25498015f, 1f);

            style.WindowRounding = 4;
            style.FrameRounding = 4;
            style.PopupRounding = 4;
            style.GrabRounding = 2;
        }
    }
}
