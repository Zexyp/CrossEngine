using System;
using static Evergine.Bindings.Imgui.ImguiNative;
using Evergine.Bindings.Imgui;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

using CrossEngine.Display;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using CrossEngine.Services;
using CrossEngine.Events;
using CrossEngine.Logging;

#if WASM
using CrossEngine.Platform.Wasm;
#endif

#if WINDOWS
using CrossEngine.Platform.Windows;
using CrossEngine.Utils.ImGui;
using CrossEngine.Rendering.Shaders;
using System.Diagnostics;
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
            public SusQ()
            {
                Manager.Register(new TimeSevice());
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
                Manager.Register(new InputService());
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
#if true
                    unsafe
                    {
                        var rapi = Manager.GetService<RenderService>().RendererApi;
                        rapi.SetClearColor(Vector4.One / 2);

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
                        //LineRenderer.Init(rapi);
                    }
#elif WASM
                    var vertSrc = @"#version 300 es

layout(location = 0) in highp vec2 in_xy;
layout(location = 1) in highp vec3 in_rgb;

out highp vec3 color;

// GLSL uses the reverse order to a System.Numerics.Matrix3x2
uniform mat2x3 viewprojection;

void main()
{
	gl_Position = vec4(vec3(in_xy, 1.0) * viewprojection, 0.0, 1.0);
	color = in_rgb;
}
";
                    var fragSrc = @"#version 300 es

in highp vec3 color;

layout(location = 0) out highp vec4 diffuse;

void main()
{
	diffuse = vec4(color, 1.0);
}
";

                    d = new WebGL.Sample.MeshDemo(CrossEngine.Platform.Wasm.EGLContext.gl, vertSrc, fragSrc);

                    //CrossEngine.Platform.Wasm.EGLContext.gl.Viewport(0, 0, (uint)100, (uint)100);
#endif
                });

                rs.Frame += () =>
                {
                    unsafe
                    {
#if true
                        var rapi = Manager.GetService<RenderService>().RendererApi;

                        CrossEngine.Platform.OpenGL.Debugging.GLError.Call(() =>
                        {
                            rapi.Clear();
                            rapi.DrawArray(new WeakReference<VertexArray>(va), 3);
                        });

                        Renderer2D.BeginScene(System.Numerics.Matrix4x4.Identity);
                        Renderer2D.DrawQuad(System.Numerics.Matrix4x4.Identity, System.Numerics.Vector4.One);
                        Renderer2D.EndScene();

                        //LineRenderer.BeginScene(System.Numerics.Matrix4x4.Identity);
                        //LineRenderer.DrawCircle(System.Numerics.Matrix4x4.Identity, System.Numerics.Vector4.UnitX);
                        //LineRenderer.EndScene();

                        /*
                        byte o = 1;
                        igShowDemoWindow(&o);

                        igBegin("sus", &o, ImGuiWindowFlags.None);
                        igText(Time.DeltaTime.ToString());
                        igEnd();
                        */
#elif WASM
                        d.Render();
#endif
                    }
                };
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                //Console.WriteLine("upd");
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
