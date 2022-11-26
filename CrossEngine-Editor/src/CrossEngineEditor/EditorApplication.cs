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
using CrossEngine.Platform.OpenGL.Debugging;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor
{
    class EditorApplication : Application
    {
        public EditorApplication() : base("CrossEngine Editor")
        {
        }

        protected override void Init()
        {
            base.Init();
            PushOverlay(new ImGuiLayer());
            PushLayer(new EditorLayer());
        }

        protected override void RenderInit()
        {
            base.RenderInit();

            GLDebugging.Enable(LogLevel.Warn);
            Renderer2D.Init();
            LineRenderer.Init();
            RendererAPI.SetDepthFunc(DepthFunc.Default);
            RendererAPI.SetBlendFunc(BlendFunc.OneMinusSrcAlpha);
        }

        protected override void RenderDestroy()
        {
            Renderer2D.Shutdown();
            LineRenderer.Shutdown();

            base.RenderDestroy();
        }
    }
}
