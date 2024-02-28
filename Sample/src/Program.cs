using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Timers;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using System.Runtime.Loader;
using System.Reflection;

using CrossEngine;
using CrossEngine.Display;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using CrossEngine.Services;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Scenes;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Ecs;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Components;
using CrossEngine.Systems;
using CrossEngine.Debugging;
using CrossEngine.Inputs;
using CrossEngine.Serialization;
using CrossEngine.Serialization.Json;
using CrossEngine.Core;
using CrossEngineRuntime;
using CrossEngine.Assets;
using CrossEngine.Platform;

using Sample.Components;

#if WASM
using CrossEngine.Platform.Wasm;
#endif

#if WINDOWS
using CrossEngine.Platform.Windows;
using CrossEngine.Platform.OpenGL.Debugging;
using ImGuiNET;
#endif

namespace Sample
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            var app = new SampleApp();
#if WASM
            app.OnInit();
#else
            app.Run();
#endif
            GPUGC.PrintCollected();
        }
    }

    class SampleApp : RuntimeApplication
    {
        public static float MaxDistance;
        public static float Distance;

        static SampleApp Instance;

        public override void OnInit()
        {
            base.OnInit();

            Debug.Assert(Instance == null);
            Instance = this;

            var os = Manager.GetService<OverlayService>();
            os.AddOverlay(new HelloOverlay());
#if DEBUG
            os.AddOverlay(new DebugOverlay());
#endif
        }

        class HelloOverlay : Overlay
        {
            protected override void Content()
            {
                MaxDistance = Math.Max(Distance, MaxDistance);
                var cornerMat = Matrix4x4.CreateTranslation(new(-Size.X / 2, Size.Y / 2, 0));
                var mat = Matrix4x4.CreateScale(2) * cornerMat;
                TextRendererUtil.DrawText(mat, $"Distance: {Distance / 6:0.00} m", Vector4.One);
                mat = Matrix4x4.CreateScale(1.5f) * Matrix4x4.CreateTranslation(new(0, -TextRendererUtil.SymbolHeight * 2, 0)) * cornerMat;
                TextRendererUtil.DrawText(mat, $"Best: {MaxDistance / 6:0.00} m", Vector4.One);

                if (PipeManagerComponent.stop)
                {
                    TextRendererUtil.DrawText(Matrix4x4.CreateScale(2), "Press 'Space' to retry", Vector4.One);
                    if (Input.GetKeyDown(Key.Space) || Input.GetMouseDown(Button.Left))
                    {
                        Distance = 0;

                        PipeManagerComponent.stop = false;
                        PipeManagerComponent.start = false;
                        
                        SceneManager.Stop(Instance.scene);
                        SceneManager.Unload(Instance.scene);
                        AssetManager.UnloadAsync().ContinueWith(t =>
                        {
                        Instance.scene = null;
                        AssetManager.LoadAsync().ContinueWith(t =>
                        {
                        SceneManager.Load(Instance.scene = AssetManager.GetNamed<SceneAsset>("sc-main").Scene);
                        SceneManager.Start(Instance.scene);
                        });
                        });
                    }
                }
            }
        }

        class DebugOverlay : Overlay
        {
            float scale = .75f;
            protected override void Content()
            {
                var t =
$@"{1d / Time.UnscaledDelta:000.00} fps
{Time.UnscaledDelta * 1000:00.000} ms
quad stat: {Renderer2D.data.quads.Stats.ItemCount} => {Renderer2D.data.quads.Stats.DrawCalls}
stop: {PipeManagerComponent.stop}
start: {PipeManagerComponent.start}";
                Renderer2D.ResetStats();
                var height = t.Count(ch => ch == '\n') + 1;
                var cornerMat = Matrix4x4.CreateTranslation(new(-Size.X / 2, -Size.Y / 2, 0));
                var mat = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(new Vector3(0, height * TextRendererUtil.SymbolHeight * scale, 0)) * cornerMat;
                TextRendererUtil.DrawText(mat, t, new Vector4(1, 0, 0, 1));
            }
        }
    }
}
