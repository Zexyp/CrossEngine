using System;

using CrossEngine;
using CrossEngine.Utils;
using CrossEngine.Layers;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Lines;
using CrossEngine.Rendering.Passes;

namespace CrossEngineEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new EditorApplication();
            app.Run();
        }
    }

    class EditorApplication : Application
    {
        public EditorApplication() : base("Editor")
        {
            PushOverlay(new ImGuiLayer());
            PushLayer(new EditorLayer());
            //PushLayer(sceneLayer = new SceneLayer(null));
            //PushLayer(new TestLayer());
            //PushLayer(new SceneTestLayer());
        }

        protected override void Init()
        {
            LineRenderer.Init();
            Renderer2D.Init();
            Renderer.RegisterPass(new SpriteRenderPass());
            Renderer.RegisterPass(new LineRenderPass());
            Renderer.EnableDepthTest(true);
            Log.EnableGLDebugging(Logger.Level.Warn);
            //Renderer.SetClearColor(0.05f, 0.05f, 0.05f);
            Renderer.SetClearColor(0.75f, 0.75f, 0.75f);
        }
    }
}
