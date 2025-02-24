using System;
using Silk.NET.GLFW;
using Silk.NET.OpenGL;

using CrossEngine.Rendering;
using CrossEngine.Logging;
using static CrossEngine.Platform.Glfw.GlfwWindow;

namespace CrossEngine.Platform.OpenGL
{
    internal unsafe class GLContext : GraphicsContext
    {
        WindowHandle* _window = null;

        public static Silk.NET.OpenGL.GL gl;

        internal static Func<string, IntPtr> loader;

        public unsafe GLContext(WindowHandle* window)
        {
            _window = window;
        }

        public override unsafe void Init()
        {
            glfw.MakeContextCurrent(_window);
            loader = glfw.GetProcAddress;


            gl = GL.GetApi(loader);
            GLExtensions.Load();

#if DEBUG
            Debugging.GLDebugging.Enable();
#endif
        }

        public override void MakeCurrent()
        {
            glfw.MakeContextCurrent(_window);
        }

        public unsafe override void SwapBuffers()
        {
            glfw.SwapBuffers(_window);
        }

        public override void Shutdown()
        {
            
        }
    }
}
