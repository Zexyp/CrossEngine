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
using CrossEngine.Ecs.Components;
using System.Runtime.CompilerServices;

#if WASM
using CrossEngine.Platform.Wasm;
#endif

#if WINDOWS
using CrossEngine.Platform.Windows;
using CrossEngine.Platform.OpenGL.Debugging;
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
            //Log.Default.Color = 0x25ff82;
            Log.Default.Trace("logging goes brr");
            Log.Default.Debug("logging goes brr");
            Log.Default.Info("logging goes brr");
            Log.Default.Warn("logging goes brr");
            Log.Default.Error("logging goes brr");
            Log.Default.Fatal("logging goes brr");

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
                Manager.Register(new TimeService());
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
                        GLFW.Glfw.SwapInterval(4);
                        ImGui.ShowDemoWindow();
                        static void DrawVec3Control(string label, ref Vector3 values, float resetValue = 0.0f, float columnWidth = 100.0f)
	                    {
		                    ImGuiIOPtr io = ImGui.GetIO();
                            var font = ImGui.GetFont();
                            var style = ImGui.GetStyle();
                            var boldFont = io.Fonts.Fonts[0];

		                    ImGui.PushID(label);

		                    ImGui.Columns(4, label, false);
		                    ImGui.Text(label);
		                    
                            ImGui.NextColumn();

		                    float lineHeight = font.FontSize + style.FramePadding.Y * 2.0f;
		                    Vector2 buttonSize = new Vector2(lineHeight + 3.0f, lineHeight);

		                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.1f, 0.15f, 1.0f));
		                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.2f, 0.2f, 1.0f));
		                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.8f, 0.1f, 0.15f, 1.0f));
		                    ImGui.PushFont(boldFont);
		                    if (ImGui.Button("X", buttonSize))
			                    values.X = resetValue;
		                    ImGui.PopFont();
		                    ImGui.PopStyleColor(3);

		                    ImGui.SameLine();
		                    ImGui.DragFloat("##X", ref values.X, 0.1f, 0.0f, 0.0f, "%.2f");
		                    ImGui.SameLine();
                            
                            ImGui.NextColumn();

                            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.7f, 0.2f, 1.0f));
		                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.8f, 0.3f, 1.0f));
		                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.2f, 0.7f, 0.2f, 1.0f));
		                    ImGui.PushFont(boldFont);
		                    if (ImGui.Button("Y", buttonSize))
			                    values.Y = resetValue;
		                    ImGui.PopFont();
		                    ImGui.PopStyleColor(3);

                            ImGui.SameLine();
		                    ImGui.DragFloat("##Y", ref values.Y, 0.1f, 0.0f, 0.0f, "%.2f");
		                    ImGui.SameLine();
                            
                            ImGui.NextColumn();

		                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.1f, 0.25f, 0.8f, 1.0f));
		                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.2f, 0.35f, 0.9f, 1.0f));
		                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.1f, 0.25f, 0.8f, 1.0f));
		                    ImGui.PushFont(boldFont);
		                    if (ImGui.Button("Z", buttonSize))
			                    values.Z = resetValue;
		                    ImGui.PopFont();
		                    ImGui.PopStyleColor(3);

		                    ImGui.SameLine();
		                    ImGui.DragFloat("##Z", ref values.Z, 0.1f, 0.0f, 0.0f, "%.2f");

		                    ImGui.PopStyleVar();

		                    ImGui.Columns(1);

		                    ImGui.PopID();
	                    }
                        if (ImGui.Begin("sus"))
                        {
                            var asd = Vector3.Zero;
                            DrawVec3Control("yeet", ref asd);
                            if (ImGui.Button("go"))
                                Time.Scale = 1;
                            if (ImGui.Button("halt"))
                                Time.Scale = 0;
                            ImGui.End();
                        }
#endif
                    }
                };

                scene = new Scene();
                camEnt = scene.CreateEntity();
                var camComp = camEnt.AddComponent(new OrthographicCameraComponent() { Size = 10 });
                camComp.Primary = true;
                entity = scene.CreateEntity();
                entity.AddComponent(new SpriteRendererComponent());
                scene.CreateEntity().Parent = entity;
                entity.Children[0].AddComponent(new SpriteRendererComponent());
                SceneManager.Load(scene);
                Time.FixedUnscaledDelta = 1;
                Manager.GetService<TimeService>().FixedUpdate += _ => Console.WriteLine("fixed");
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
                    SceneManager.Unload();
                }
            }
        }
    }
}
