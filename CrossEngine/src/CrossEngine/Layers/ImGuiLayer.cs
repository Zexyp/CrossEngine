using System;
using ImGuiNET;
using ImGuizmoNET;

using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Utils;
using CrossEngine.Logging;

namespace CrossEngine.Layers
{
    public class ImGuiLayer : Layer
    {
        public ImGuiLayer()
        {
            Instance = this;
        }

        static public ImGuiLayer Instance;

        public bool BlockEvents = true;

        IntPtr ImGuiContext;

        public unsafe override void OnAttach()
        {
            ImGuiContext = ImGui.CreateContext();
            //ImGui.SetCurrentContext(context);
            ImGuizmo.SetImGuiContext(ImGuiContext);

            Application app = Application.Instance;
            GLFW.Window windowHandle = app.Window.Handle;

            if (ImGuiController.ImGui_ImplGlfw_InitForOpenGL(windowHandle, true)) Log.Core.Info("ImGui GLFW part initialized");
            if (ImGuiController.ImGui_ImplOpenGL3_Init("#version 330 core")) Log.Core.Info("ImGui OpenGL part initialized");

            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(
                app.Window.Width / Vector2.One.X,
                app.Window.Height / Vector2.One.Y);
            io.DisplayFramebufferScale = Vector2.One;
            io.DeltaTime = 1.0f / 60.0f;

            //ImGui.StyleColorsClassic();
            //ImGui.StyleColorsLight();
            ImGui.StyleColorsDark();
            SetDarkThemeColors();

            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            //io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;       // Enable Keyboard Controls
            ////io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;      // Enable Gamepad Controls
            //io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;           // Enable Docking
            //io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;         // Enable Multi-Viewport / Platform Windows
            ////io.ConfigFlags |= ImGuiConfigFlags.ViewportsNoTaskBarIcons;
            ////io.ConfigFlags |= ImGuiConfigFlags.ViewportsNoMerge;

            //io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
            //io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;



            //io.Fonts.AddFontDefault(io.Fonts.AddFontFromFileTTF(FileEnvironment.ResourceFolder + "fonts/consola.ttf", 16.0f, null, io.Fonts.GetGlyphRangesChineseSimplifiedCommon()).ConfigData);

            //ImFontAtlasPtr fonts = ImGui.GetIO().Fonts;
            //io.Fonts.AddFontDefault();
            //io.Fonts.AddFontFromFileTTF(FileEnvironment.ResourceFolder + "fonts/arial.ttf", 18.0f);
            //io.Fonts.AddFontDefault(io.Fonts.AddFontFromFileTTF(FileEnvironment.ResourceFolder + "fonts/arial.ttf", 18.0f).ConfigData); //io.FontDefault = io.Fonts.AddFontFromFileTTF(FileEnvironment.ResourceFolder + "fonts/arial.ttf", 18.0f);
            //
            //
            //
            //ImGuiStylePtr style = ImGui.GetStyle();
            //if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) > 0)
            //{
            //    style.WindowRounding = 0.0f;
            //    style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
            //}
            //
        }

        public override void OnDetach()
        {
            ImGui.DestroyPlatformWindows();
            //ImGui.*DestroyContext*(ImGuiContext); // this somehow destroys to much
            ImGuiContext = IntPtr.Zero;
            Log.Core.Info("ImGui context destroyed");
            ImGuiController.ImGui_ImplOpenGL3_Shutdown();
            ImGuiController.ImGui_ImplGlfw_Shutdown();
        }

        public override void OnEvent(Event e)
        {
            if (BlockEvents)
            {
                ImGuiIOPtr io = ImGui.GetIO();
                e.Handled |= e is MouseEvent && io.WantCaptureMouse;
                e.Handled |= e is KeyEvent && io.WantCaptureKeyboard;
                e.Handled |= e is KeyEvent && io.WantTextInput;
            }
        }

        public void Begin()
        {
            ImGuiController.ImGui_ImplOpenGL3_NewFrame();
            ImGuiController.ImGui_ImplGlfw_NewFrame();
            ImGui.NewFrame();
            ImGuizmo.BeginFrame();
        }

        public void End()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            Application app = Application.Instance;
            io.DisplaySize = new Vector2((float)app.Window.Width, (float)app.Window.Height);

            // Rendering
            ImGui.Render();
            ImGuiController.ImGui_ImplOpenGL3_RenderDrawData(ImGui.GetDrawData());

            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) > 0)
            {
                GLFW.Window backup_current_context = GLFW.Glfw.CurrentContext;
                ImGui.UpdatePlatformWindows();
                ImGui.RenderPlatformWindowsDefault();
                GLFW.Glfw.MakeContextCurrent(backup_current_context);
            }
        }

        void SetDarkThemeColors()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;
            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.1f, 0.105f, 0.11f, 1.0f);

            // headers
            colors[(int)ImGuiCol.Header] = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

            // buttons
            colors[(int)ImGuiCol.Button] = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

            // frame bg
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

            // tabs
            colors[(int)ImGuiCol.Tab] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.38f, 0.3805f, 0.381f, 1.0f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.28f, 0.2805f, 0.281f, 1.0f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);

            // title
            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.102f, 0.251f, 0.42f, 1.0f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.0f, 0.298f, 0.498f, 1.0f); //new Vector4(0.0f, 0.424f, 0.71f, 1.0f);
            style.TabRounding = 3f;
            style.FrameRounding = 4f;
            style.GrabRounding = 4f;
            style.WindowMenuButtonPosition = ImGuiDir.Right;
        }
    }
}
