using CrossEngine;
using CrossEngine.Core;
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
using System.Diagnostics;
using CrossEngine.Debugging;
using CrossEngine.Inputs;
using CrossEngine.Utils;
using System.Numerics;
using CrossEngine.Display;
using CrossEngine.Scenes;

namespace CrossEngineEditor
{
    internal class EditorApplication : Application
    {
        public static EditorApplication Instance;

        public EditorApplication()
        {
            Debug.Assert(Instance == null);
            Instance = this;

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
            //Manager.Register(new AssetService());
            Manager.Register(new OverlayService(mo = new MetricsOverlay()));

            Manager.Register(new EditorService());

            // configure
        }

        class MetricsOverlay : HudOverlay
        {
            public override void Draw()
            {
                Renderer2D.SetBlending(BlendMode.Blend);
                base.Draw();
            }

            protected override void Content()
            {
                var t = $"{1d / Time.UnscaledDelta:000.00} fps\n{Time.UnscaledDelta * 1000:00.000} ms\n";
                TextRendererUtil.DrawText(Matrix4x4.Identity, t, ColorHelper.U32ToVec4(0x7fff006d));
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }
    }
}
