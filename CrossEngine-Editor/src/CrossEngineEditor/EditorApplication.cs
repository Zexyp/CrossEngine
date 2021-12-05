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
using CrossEngine.Rendering.Lines;
using CrossEngine.Profiling;

using CrossEngineEditor.Utils;

namespace CrossEngineEditor
{
    class EditorApplication : Application
    {
        public static Logger Log = CrossEngine.Logging.Log.GetLogger("EDITOR");

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
            CrossEngine.Logging.Log.EnableGLDebugging(LogLevel.Warn);
            //Renderer.SetClearColor(0.05f, 0.05f, 0.05f);
            Renderer.SetClearColor(0.75f, 0.75f, 0.75f);
        }
    }
}
