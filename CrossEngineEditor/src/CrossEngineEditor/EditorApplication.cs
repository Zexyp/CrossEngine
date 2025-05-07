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
            Manager.Register(new OverlayService(mo = new MetricsOverlay()));

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

        class MetricsOverlay : HudOverlay
        {
            private float[] _deltas = new float[256];
            private int _deltasIndex = 0;
            
            public override void Draw()
            {
                Renderer2D.SetBlending(BlendMode.Blend);
                GraphicsContext.Current.Api.SetDepthFunc(DepthFunc.None);
                base.Draw();
            }

            protected override void Content()
            {
                // text
                var t = $"{1d / Time.UnscaledDelta:000.00} fps\n{Time.UnscaledDelta * 1000:00.000} ms\n[{(Debugger.IsAttached ? "debug" : "alone")}]";
                var offset = new Vector3(0, Size.Y - TextRendererUtil.TextRendererUtilData.SymbolHeight * 3, 0);
                TextRendererUtil.DrawText(Matrix4x4.CreateTranslation(offset), t, ColorHelper.U32ToVec4(0x7fff006d));

                // graph
                var graphY = Size.Y - TextRendererUtil.TextRendererUtilData.SymbolHeight * 3;
                var graphX = 0;
                _deltas[_deltasIndex] = Time.UnscaledDeltaF;
                
                for (int ioff = 0; ioff < _deltas.Length; ioff++)
                {
                    var i = (_deltasIndex + ioff) % _deltas.Length;
                    Renderer2D.DrawQuad(Matrix4x4.CreateTranslation(new Vector3(0, 0.5f, 0)) *
                                        Matrix4x4.CreateScale(new Vector3(1, _deltas[i] * 1000, 1)) *
                                        Matrix4x4.CreateTranslation(new Vector3(ioff + graphX, -_deltas[i] * 1000 + graphY, 0)),
                        new Vector4(1, 0, 0, .5f));
                }
                
                _deltasIndex++;
                _deltasIndex %= _deltas.Length;
            }
        }
    }
}
