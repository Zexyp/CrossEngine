using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine;
using CrossEngine.Utils;
using CrossEngine.Layers;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Profiling;
using CrossEngine.Debugging;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor
{
    class EditorApplication : Application
    {
        public EditorApplication() : base("CrossEngine Editor")
        {
            PushOverlay(new ImGuiLayer());
            PushLayer(new EditorLayer());
        }

        protected override void RenderInit()
        {
            base.RenderInit();

            GLDebugging.EnableGLDebugging(LogLevel.Warn);
            Renderer2D.Init();
            LineRenderer.Init();
            RendererAPI.SetDepthFunc(DepthFunc.Default);
            RendererAPI.SetBlendFunc(BlendFunc.OneMinusSrcAlpha);
        }

        protected override void RenderDestroy()
        {
            base.RenderDestroy();

            Renderer2D.Shutdown();
            LineRenderer.Shutdown();
        }
    }
}
