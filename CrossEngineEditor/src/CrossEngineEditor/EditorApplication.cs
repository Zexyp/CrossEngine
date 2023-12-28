using CrossEngine;
using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Services;
using CrossEngine.Utils.ImGui;
using CrossEngineEditor.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using CrossEngine.Logging;

namespace CrossEngineEditor
{
    internal class EditorApplication : Application
    {
        public EditorApplication()
        {
            Manager.Register(new TimeService());
            Manager.Register(new InputService());
            Manager.Register(new WindowService(
                WindowService.Mode.ThreadLoop
                ));
            Manager.Register(new RenderService(
                GraphicsApi.OpenGL
                ));
            Manager.Register(new ImGuiService());
            Manager.Register(new SceneService());
            Manager.Register(new EditorService());
            Manager.GetService<WindowService>().WindowEvent += OnEvent;
        }

        private void OnEvent(WindowService ws, Event e)
        {
            if (e is WindowCloseEvent)
                Close();
        }
    }
}
