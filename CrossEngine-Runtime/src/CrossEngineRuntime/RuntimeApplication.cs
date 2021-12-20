using CrossEngine;
using CrossEngine.Layers;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Lines;
using CrossEngine.Logging;

namespace CrossEngineRuntime
{
    class RuntimeApplication : Application
    {
        public RuntimeApplication() : base("Runtime")
        {
            PushOverlay(new ImGuiLayer());
            PushLayer(new SceneLayer());
        }

        protected override void Init()
        {
            LineRenderer.Init();
            Renderer2D.Init();
            //Renderer.EnableDepthTest(true);
            Log.EnableGLDebugging(LogLevel.Warn);
            Renderer.SetClearColor(0.05f, 0.05f, 0.05f);
        }
    }
}
