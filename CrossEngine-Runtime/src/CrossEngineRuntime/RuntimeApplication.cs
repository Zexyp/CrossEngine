using System.IO;

using CrossEngine;
using CrossEngine.Layers;
using CrossEngine.Rendering;
using CrossEngine.Logging;
using CrossEngine.Debugging;

namespace CrossEngineRuntime
{
    class RuntimeApplication : Application
    {        
        public RuntimeApplication() : base("Runtime")
        {
            ThreadManager.ExecuteOnRenderThread(() =>
            {
                GLDebugging.EnableGLDebugging(LogLevel.Warn);
                Renderer2D.Init();
                LineRenderer.Init();
                RendererAPI.SetDepthFunc(DepthFunc.Default);
                RendererAPI.SetBlendFunc(BlendFunc.OneMinusSrcAlpha);
            });

            PushOverlay(new ImGuiLayer());
            PushLayer(new RuntimeLayer());
        }
    }
}
