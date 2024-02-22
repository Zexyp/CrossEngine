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

namespace CrossEngineEditor
{
    internal class EditorApplication : Application
    {
        public static EditorApplication Instance;

        public EditorApplication()
        {
            Debug.Assert(Instance == null);
            Instance = this;

            // provides a nice smooth shutdown
            //AppDomain.CurrentDomain.ProcessExit <- this is nice but works like ass
            Console.CancelKeyPress += (object? sender, ConsoleCancelEventArgs e) =>
            {
                CloseWait();
                Environment.Exit(25);
            };

            // register
            Manager.Register(new TimeService());
            Manager.Register(new InputService());
            Manager.Register(new WindowService(
                WindowService.Mode.ThreadLoop
                ));
            Manager.Register(new RenderService());
            Manager.Register(new ImGuiService("res/fonts/JetBrainsMono[wght].ttf"));
            Manager.Register(new SceneService());
            Manager.Register(new AssetService());

            //Manager.Register(new OverlayService());

            Manager.Register(new EditorService());


            // configure
            Manager.GetService<WindowService>().WindowEvent += OnEvent;
            //Manager.GetService<OverlayService>().AddOverlay(new HelloOverlay());
        }

        class HelloOverlay : Overlay
        {
            protected override void Content()
            {
                var mat = Matrix4x4.CreateTranslation(new Vector3(-Size.X / 2, Size.Y / 2, 0));
                var t = $"{1d / Time.UnscaledDelta:000.00} fps\n{Time.UnscaledDelta * 1000:00.000} ms";
                TextRendererUtil.DrawText(mat, t, new Vector4(1, 0, 0, 1));
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Instance = null;
        }

        private void OnEvent(WindowService ws, Event e)
        {
            if (e is WindowCloseEvent)
                Close();
        }
    }
}
