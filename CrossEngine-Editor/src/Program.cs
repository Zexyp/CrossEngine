using System;

using CrossEngine;
using CrossEngine.Utils;
using CrossEngine.Layers;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Lines;
using CrossEngine.Profiling;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor
{
    class Program
    {
        static void Main(string[] args)
        {
            Profiler.BeginSession("Main", "profiling.json");
            var app = new EditorApplication();
            app.Run();
            Profiler.EndSession();
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
            Log.EnableGLDebugging(Logger.Level.Warn);
            //Renderer.SetClearColor(0.05f, 0.05f, 0.05f);
            Renderer.SetClearColor(0.75f, 0.75f, 0.75f);
        }
    }
}
