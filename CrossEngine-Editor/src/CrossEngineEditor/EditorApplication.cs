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
        public static Logger Log = CrossEngine.Logging.Log.GetLogger("EDITOR");

        public EditorApplication() : base("CrossEngine Editor")
        {
            PushOverlay(new ImGuiLayer());
            PushLayer(new SceneLayer());
            PushLayer(new EditorLayer());
        }
    }
}
