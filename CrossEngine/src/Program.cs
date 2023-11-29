using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Diagnostics;

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

#if WASM
using CrossEngine.Platform.Wasm;
#endif

#if WINDOWS
using CrossEngine.Platform.Windows;
using CrossEngine.Platform.OpenGL.Debugging;
using System.Runtime.CompilerServices;
using ImGuiNET;
#endif

namespace CrossEngine
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //System.Runtime.CompilerServices.Unsafe.AsRef<bool>(null);
            Log.Print(LogLevel.Trace, "logging goes brr");
            Log.Print(LogLevel.Debug, "logging goes brr");
            Log.Print(LogLevel.Info,  "logging goes brr");
            Log.Print(LogLevel.Warn,  "logging goes brr");
            Log.Print(LogLevel.Error, "logging goes brr");
            Log.Print(LogLevel.Fatal, "logging goes brr");

            var app = new SusQ();
#if WASM
            app.OnInit();
#else
            app.Run();
#endif
            GPUGC.PrintCollected();
        }

        class SusQ : Application
        {
            Scene scene;
            Entity entity;
            Entity camEnt;

            public SusQ()
            {
                Manager.Register(new TimeSevice());
                Manager.Register(new InputService());
                Manager.Register(new WindowService(
#if WASM
                    WindowService.Mode.Manual
#else
                    WindowService.Mode.ThreadLoop
#endif
                    ));
                Manager.Register(new RenderService(
#if WASM
                    GraphicsApi.OpenGLES
                    ) { IgnoreRefresh = true });
#else
                    GraphicsApi.OpenGL
                    ));
#endif                    
#if WINDOWS
                Manager.Register(new CrossEngine.Utils.ImGui.ImGuiService());
#endif
                Manager.Register(new SceneService());

                Manager.GetService<InputService>().Event += OnEvent;
#if WASM
                Manager.GetService<WindowService>().Event += (e) =>
                {
                    if (e is WindowRefreshEvent)
                    {
                        OnUpdate();
                        Manager.GetService<WindowService>().Update();
                    }
                };
#endif
            }

            VertexArray va;

            public override void OnInit()
            {
                base.OnInit();

                //Manager.GetService<RenderService>().Frame += () => Console.WriteLine("rnd");

                var rs = Manager.GetService<RenderService>();
                rs.Execute(() =>
                {
                    unsafe
                    {
                        var rapi = Manager.GetService<RenderService>().RendererApi;

                        float[] vertices = {
                            -0.5f, -0.5f, 0.0f, // left  
                             0.5f, -0.5f, 0.0f, // right 
                             0.0f,  0.5f, 0.0f  // top   
                        };
                        va = VertexArray.Create().GetValue();
                        VertexBuffer vb;
                        fixed (void* p = &vertices[0])
                            vb = VertexBuffer.Create(p, (uint)(vertices.Length * sizeof(float))).GetValue();
                        vb.SetLayout(new BufferLayout(new BufferElement(Rendering.Shaders.ShaderDataType.Float3, "pos")));
                        va.AddVertexBuffer(new WeakReference<VertexBuffer>(vb));

                        Renderer2D.Init(rapi);
                        LineRenderer.Init(rapi);
                        TextRendererUtil.Init();
                    }
                });

                Stopwatch sw = Stopwatch.StartNew();
                rs.Frame += (rs) =>
                {
                    unsafe
                    {
                        var rapi = rs.RendererApi;
                        var ws = Manager.GetService<WindowService>();

                        //rapi.Clear();
                        //rapi.DrawArray(new WeakReference<VertexArray>(va), 3);

                        Renderer2D.BeginScene(Matrix4x4.Identity);

                        //TextRendererUtil.DrawText(Matrix4x4.Identity, "@abc", new Vector4(1, 0, 0, 1));

                        Renderer2D.EndScene();

                        LineRenderer.BeginScene(((ICamera)scene.World.GetSystem<RenderSystem>().PrimaryCamera).ViewProjectionMatrix);
                        LineRenderer.DrawCircle(System.Numerics.Matrix4x4.Identity, System.Numerics.Vector4.UnitX);
                        scene.World.GetSystem<TransformSystem>().DebugRender();
                        LineRenderer.EndScene();

#if WINDOWS
                        ImGui.ShowDemoWindow();
                        //
                        //bool o = true;
                        //ImGui.Begin("sus", ref o, ImGuiWindowFlags.None);
                        //
                        //Vector4 vec = new Vector4();
                        //var style = ImGui.GetStyle();
                        //ImGui.Columns(4);
                        //
                        //ImGui.SliderFloat("##X", ref vec.X, -10.0f, 10.0f, null, ImGuiSliderFlags.None);
                        //ImGui.SameLine();
                        //ImGui.PushStyleColor(ImGuiCol.Text, 0xff0000ff);
                        //ImGui.Text("X");
                        //ImGui.PopStyleColor(1);
                        //
                        //ImGui.NextColumn();
                        //
                        //ImGui.SliderFloat("##Y", ref vec.Y, -10.0f, 10.0f, null, ImGuiSliderFlags.None);
                        //ImGui.SameLine();
                        //ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ff00);
                        //ImGui.Text("Y");
                        //ImGui.PopStyleColor(1);
                        //
                        //ImGui.NextColumn();
                        //
                        //ImGui.SliderFloat("##Z", ref vec.Z, -10.0f, 10.0f, null, ImGuiSliderFlags.None);
                        //ImGui.SameLine();
                        //ImGui.PushStyleColor(ImGuiCol.Text, 0xffff0000);
                        //ImGui.Text("Z");
                        //ImGui.PopStyleColor(1);
                        //
                        //ImGui.NextColumn();
                        //
                        //ImGui.SliderFloat("##W", ref vec.W, -10.0f, 10.0f, null, ImGuiSliderFlags.None);
                        //ImGui.SameLine();
                        //ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ffff);
                        //ImGui.Text("W");
                        //ImGui.PopStyleColor(1);
                        //
                        //ImGui.Columns(0, null, false);
                        //
                        //ImGui.Text(Time.Delta.ToString());
                        //ImGui.End();
#endif
                    }
                };

                scene = new Scene();
                camEnt = scene.CreateEntity();
                var camComp = camEnt.AddComponent(new CameraComponent() { ProjectionMatrix = Matrix4x4.CreateScale(.1f) });
                camComp.Primary = true;
                entity = scene.CreateEntity();
                entity.AddComponent(new SpriteRendererComponent());
                scene.CreateEntity().Parent = entity;
                entity.Children[0].AddComponent(new SpriteRendererComponent());
                SceneManager.Load(scene);
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                //Console.WriteLine("upd");
                entity.Children[0].Transform.Position = Vector3.UnitY * MathF.Sin(Time.ElapsedF * 4) * 5 + Vector3.UnitX * MathF.Cos(Time.ElapsedF * 4) * 5;
                entity.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Time.ElapsedF);
                Vector3 delta = new Vector3(
                    (Input.GetKey(Key.D) ? 1 : 0) + (Input.GetKey(Key.A) ? -1 : 0),
                    (Input.GetKey(Key.W) ? 1 : 0) + (Input.GetKey(Key.S) ? -1 : 0),
                    0)
                    *
                    (Input.GetKey(Key.LeftShift) ? 4 : 2)
                    *
                    Time.DeltaF;
                camEnt.Transform.Position += delta;
            }

            void OnEvent(Event e)
            {
                //if (e is not WindowRefreshEvent)
                //    Console.WriteLine(e);
                if (e is WindowCloseEvent)
                {
                    Close();
                }
                if (e is WindowResizeEvent wre)
                {
                    var rs = Manager.GetService<RenderService>();
                    rs.Execute(() => rs.RendererApi.SetViewport(0, 0, wre.Width, wre.Height));
                }
            }

            
        }
    }
}
