using CrossEngine.Platform.Windows;
using CrossEngine.Services;
using System;

using ImGuiNET;
using IG = ImGuiNET.ImGui;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace CrossEngine.Utils.ImGui
{
    public class ImGuiService : Service
    {
        MyImGuiController controller;
        public override void OnDestroy()
        {
            var rs = Manager.GetService<RenderService>();
            rs.BeforeFrame -= OnBeforeFrame;
            rs.AfterFrame -= OnAfterFrame;

            rs.Execute(() =>
            {
                
            });
        }

        public override unsafe void OnStart()
        {
            var rs = Manager.GetService<RenderService>();
            rs.BeforeFrame += OnBeforeFrame;
            rs.AfterFrame += OnAfterFrame;

            rs.Execute(() =>
            {
                // Setup Dear ImGui context
                IG.CreateContext(null);
                var io = IG.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;   // Enable Keyboard Controls
                //io->ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;    // Enable Gamepad Controls

                // Setup Dear ImGui style
                //IG.StyleColorsDark(IG.GetStyle());
                //ImGui::StyleColorsClassic();

                // Setup Platform/Renderer backends
                //ImplGlfw.ImGui_ImplGlfw_InitForOpenGL(((GlfwWindow)Manager.GetService<WindowService>().Window).NativeHandle, true);
                //ImplOpenGL.ImGui_ImplOpenGL3_Init("#version 330 core");
                controller = new MyImGuiController(CrossEngine.Platform.OpenGL.GLContext.gl, Manager.GetService<WindowService>().Window);
            });
        }

        private void OnBeforeFrame(RenderService rs)
        {
            //ImplOpenGL.ImGui_ImplOpenGL3_NewFrame();
            //ImplGlfw.ImGui_ImplGlfw_NewFrame();

            controller.Update(Time.UnscaledDeltaF);

        }

        private unsafe void OnAfterFrame(RenderService rs)
        {
            controller.Render();

            //ImplOpenGL.ImGui_ImplOpenGL3_RenderDrawData(IG.GetDrawData());
            //igUpdatePlatformWindows();
            //igRenderPlatformWindowsDefault(null, null);
        }
    }
}
