using System;
using static Evergine.Bindings.Imgui.ImguiNative;
using Evergine.Bindings.Imgui;

using CrossEngine.Display;
using CrossEngine.Platform.Windows;
using CrossEngine.Utils.ImGui;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using CrossEngine.Services;
using CrossEngine.Events;

namespace CrossEngine
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            System.Runtime.CompilerServices.Unsafe.AsRef<bool>(null);
            var app = new SusQ();
            app.Run();
        }

        class SusQ : Application
        {
            public SusQ()
            {
                Manager.Register(new TimeSevice());
                Manager.Register(new RenderService());
                Manager.Register(new ImGuiService());
                Manager.Register(new InputService());
                Manager.GetService<InputService>().OnEvent += OnEvent;
            }

            VertexArray va;

            protected override void OnInit()
            {
                Manager.GetService<RenderService>().Execute(() =>
                {
                    unsafe
                    {
                        var rapi = Manager.GetService<RenderService>().RendererAPI;
                        var window = Manager.GetService<RenderService>().Window as GlfwWindow;

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

                        rapi.SetClearColor(System.Numerics.Vector4.One / 2);
                    }
                });

                Manager.GetService<RenderService>().Frame += () =>
                {
                    unsafe
                    {
                        var rapi = Manager.GetService<RenderService>().RendererAPI;

                        CrossEngine.Platform.OpenGL.Debugging.GLError.Call(() =>
                        {
                            rapi.Clear();
                            rapi.DrawArray(new WeakReference<VertexArray>(va), 3);
                        });

                        Renderer2D.BeginScene(System.Numerics.Matrix4x4.Identity);
                        Renderer2D.DrawQuad(System.Numerics.Matrix4x4.Identity, System.Numerics.Vector4.One);
                        Renderer2D.EndScene();

                        LineRenderer.BeginScene(System.Numerics.Matrix4x4.Identity);
                        LineRenderer.DrawCircle(System.Numerics.Matrix4x4.Identity, System.Numerics.Vector4.UnitX);
                        LineRenderer.EndScene();
                        System.Runtime.CompilerServices.Unsafe.AsRef<bool>(null);

                        byte o = 1;
                        igShowDemoWindow(&o);

                        igBegin("sus", &o, ImGuiWindowFlags.None);
                        igText(Time.DeltaTime.ToString());
                        igEnd();
                    }
                };
            }

            protected override void OnDestroy()
            {
                
            }

            protected virtual void OnEvent(Event e)
            {
                if (e is WindowCloseEvent)
                {
                    Close();
                }
                if (e is WindowResizeEvent wre)
                {
                    var rs = Manager.GetService<RenderService>();
                    rs.Execute(() => rs.RendererAPI.SetViewport(0, 0, wre.Width, wre.Height));
                }
            }
        }
    }
}
