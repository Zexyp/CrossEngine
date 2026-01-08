using CrossEngine;
using CrossEngine.Core;
using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Core.Services;
using CrossEngine.Utils.ImGui;
using CrossEngineEditor.Panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using CrossEngine.Logging;
using System.Diagnostics;
using CrossEngine.Debugging;
using CrossEngine.Inputs;
using CrossEngine.Utils;
using System.Numerics;
using CrossEngine.Display;
using CrossEngine.Scenes;
using CrossEngine.Assets;
using CrossEngine.Utils.Rendering;

namespace CrossEngineEditor
{
    internal class EditorApplication : Application
    {
        public static EditorApplication Instance { get; private set; }
        public static EditorService Service { get; private set; }

        public EditorApplication()
        {
            RenderService rs;
            MetricsOverlay mo;
            // register
            Manager.Register(new TimeService());
            Manager.Register(new ConsoleInputService());
            Manager.Register(new InputService());
            Manager.Register(new WindowService(
                WindowService.Mode.ThreadLoop
            ));
            Manager.Register(rs = new RenderService());
            // is not nice but
            Manager.Register(new ImGuiService("res/fonts/JetBrainsMono[wght].ttf"));
            Manager.Register(new SceneService());
            Manager.Register(new AssetService());
            Manager.Register(new OverlayService(mo = new MetricsOverlay(rs)));

            // yippee
            Manager.Register(new EditorService());

            // configure
        }
        
        public override void OnInit()
        {
            Debug.Assert(Instance == null && Service == null);
            
            Instance = this;
            Service = Manager.GetService<EditorService>();
        }

        public override void OnDestroy()
        {
            Instance = null;
            Service = null;
        }
    }
}
