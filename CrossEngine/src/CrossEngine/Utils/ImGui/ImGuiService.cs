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
        public override void OnDetach()
        {
            var rs = Manager.GetService<RenderService>();
            rs.BeforeFrame -= OnBeforeFrame;
            rs.AfterFrame -= OnAfterFrame;

            rs.Execute(() =>
            {
                // TODO: dispose
            });
        }

        public override unsafe void OnAttach()
        {
            var rs = Manager.GetService<RenderService>();
            rs.BeforeFrame += OnBeforeFrame;
            rs.AfterFrame += OnAfterFrame;

            rs.Execute(() =>
            {
                controller = new MyImGuiController(
                    CrossEngine.Platform.OpenGL.GLContext.gl,
                    Manager.GetService<WindowService>().Window,
                    () =>
                    {
                        var io = IG.GetIO();
                        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;    // Enable Docking
                    });
            });
        }

        private void OnBeforeFrame(RenderService rs)
        {
            controller.Update(Time.UnscaledDeltaF);
        }

        private unsafe void OnAfterFrame(RenderService rs)
        {
            controller.Render();
        }

        public override void OnStart()
        {
            
        }

        public override void OnDestroy()
        {
            
        }
    }
}
