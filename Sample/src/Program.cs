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
#if WASM
using CrossEngine.Platform.Wasm;
#endif

#if WINDOWS
using CrossEngine.Platform.Windows;
using CrossEngine.Platform.OpenGL.Debugging;
using ImGuiNET;
using Sample.Components;
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

        class SampleApp : RuntimeApplication
        {
            static float MaxDistance;

            public override void OnInit()
            {
                base.OnInit();

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
                    MaxDistance = Math.Max(PipeManagerComponent.distance, MaxDistance);
                    var cornerMat = Matrix4x4.CreateTranslation(new (-Size.X / 2, Size.Y / 2, 0));
                    var mat = Matrix4x4.CreateScale(2) * cornerMat;
                    TextRendererUtil.DrawText(mat, $"Distance: {PipeManagerComponent.distance:0.00} m", new Vector4(1, 0, 0, 1));
                    mat = Matrix4x4.CreateScale(1.5f) * Matrix4x4.CreateTranslation(new (0, -TextRendererUtil.SymbolHeight * 2, 0)) * cornerMat;
                    TextRendererUtil.DrawText(mat, $"Best: {MaxDistance}", new Vector4(1, 0, 0, 1));
                }
            }

            class DebugOverlay : Overlay
            {
                protected override void Content()
                {
                    var t = 
$@"{1d / Time.UnscaledDelta:000.00} fps
{Time.UnscaledDelta * 1000:00.000} ms
distance: {PipeManagerComponent.distance}
stop: {PipeManagerComponent.stop}";
                    var height = t.Count(ch => ch == '\n') + 1;
                    var mat = Matrix4x4.CreateTranslation(new Vector3(-Size.X / 2, -Size.Y / 2 + height * TextRendererUtil.SymbolHeight, 0));
                    TextRendererUtil.DrawText(mat, t, new Vector4(1, 0, 0, 1));
                }
            }
        }
    }
}
