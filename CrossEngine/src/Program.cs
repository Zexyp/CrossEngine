using System;
using static Evergine.Bindings.Imgui.ImguiNative;
using Evergine.Bindings.Imgui;
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
using CrossEngine.Platform.OpenGL.Debugging;
using CrossEngine.Scenes;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Ecs;

#if WASM
using CrossEngine.Platform.Wasm;
#endif

#if WINDOWS
using CrossEngine.Platform.Windows;
using CrossEngine.Utils.ImGui;
#endif

namespace CrossEngine
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //System.Runtime.CompilerServices.Unsafe.AsRef<bool>(null);
            Log.Print(LogLevel.Trace, "asd");
            Log.Print(LogLevel.Debug, "asd");
            Log.Print(LogLevel.Info,  "asd");
            Log.Print(LogLevel.Warn,  "asd");
            Log.Print(LogLevel.Error, "asd");
            Log.Print(LogLevel.Fatal, "asd");

            var app = new SusQ();
#if WASM
            app.OnInit();
#else
            app.Run();
#endif
        }

        class SusQ : Application
        {
            Scene scene;
            Entity entity;

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
                Manager.Register(new ImGuiService());
#endif
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

                rs.Frame += () =>
                {
                    unsafe
                    {
                        var rapi = Manager.GetService<RenderService>().RendererApi;

                        rapi.Clear();
                        rapi.DrawArray(new WeakReference<VertexArray>(va), 3);

                        Renderer2D.BeginScene(System.Numerics.Matrix4x4.Identity);
                        
                        entity.Transform.Position = Vector3.UnitY * MathF.Sin(Time.ElapsedF);
                        entity.Children[0].Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Time.ElapsedF);
                        Renderer2D.DrawQuad(entity.Transform.WorldTransformMatrix, new Vector4(0, 1, 0, 1));
                        Renderer2D.DrawQuad(entity.Children[0].Transform.WorldTransformMatrix, new Vector4(0, 1, 0, 1));

                        Matrix4x4 textmat = Matrix4x4.CreateScale(new Vector3(32f / Manager.GetService<WindowService>().Window.Width, 32f / Manager.GetService<WindowService>().Window.Height, 1));
                        TextRendererUtil.DrawText(textmat, "@abc", new Vector4(1, 0, 0, 1));

                        Renderer2D.EndScene();

                        LineRenderer.BeginScene(System.Numerics.Matrix4x4.Identity);
                        LineRenderer.DrawCircle(System.Numerics.Matrix4x4.Identity, System.Numerics.Vector4.UnitX);
                        LineRenderer.EndScene();

                        /*
                        byte o = 1;
                        igShowDemoWindow(&o);

                        igBegin("sus", &o, ImGuiWindowFlags.None);
                        igText(Time.DeltaTime.ToString());
                        igEnd();
                        */
                    }
                };

                scene = new Scene();
                entity = scene.CreateEntity();
                scene.CreateEntity().Parent = entity;
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                //Console.WriteLine("upd");
                scene.Update();
            }

            void OnEvent(Event e)
            {
                if (e is not WindowRefreshEvent)
                    Console.WriteLine(e);
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
