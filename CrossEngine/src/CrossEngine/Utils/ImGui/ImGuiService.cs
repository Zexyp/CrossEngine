using CrossEngine.Platform.Windows;
using CrossEngine.Services;
using CrossEngine.Utils.ImGui;
using Evergine.Bindings.Imgui;
using System;
using static Evergine.Bindings.Imgui.ImguiNative;

namespace CrossEngine.Utils.ImGui
{
    public class ImGuiService : Service
    {
        public override void OnDestroy()
        {
            var rs = Manager.GetService<RenderService>();
            rs.BeforeFrame -= OnBeforeFrame;
            rs.AfterFrame -= OnAfterFrame;
        }

        public override unsafe void OnStart()
        {
            var rs = Manager.GetService<RenderService>();
            rs.BeforeFrame += OnBeforeFrame;
            rs.AfterFrame += OnAfterFrame;

            rs.Execute(() =>
            {
                // Setup Dear ImGui context
                igCreateContext(null);
                var io = igGetIO();
                io->ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;   // Enable Keyboard Controls
                //io->ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;    // Enable Gamepad Controls

                // Setup Dear ImGui style
                igStyleColorsDark(igGetStyle());
                //ImGui::StyleColorsClassic();

                // Setup Platform/Renderer backends
                ImplGlfw.ImGui_ImplGlfw_InitForOpenGL(((GlfwWindow)Manager.GetService<WindowService>().Window).NativeHandle, true);
                ImplOpenGL.ImGui_ImplOpenGL3_Init("#version 330 core");
            });
        }

        private void OnBeforeFrame(RenderService rs)
        {
            ImplOpenGL.ImGui_ImplOpenGL3_NewFrame();
            ImplGlfw.ImGui_ImplGlfw_NewFrame();
            igNewFrame();
        }

        private unsafe void OnAfterFrame(RenderService rs)
        {
            igRender();
            ImplOpenGL.ImGui_ImplOpenGL3_RenderDrawData(igGetDrawData());
            //igUpdatePlatformWindows();
            //igRenderPlatformWindowsDefault(null, null);
        }
    }
}
