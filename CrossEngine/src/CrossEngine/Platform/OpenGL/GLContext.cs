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

        [ThreadStatic]
        internal static Silk.NET.OpenGL.GL gl;

        private Func<string, IntPtr> loader;
        private Silk.NET.OpenGL.GL _gl;

        public unsafe GLContext(WindowHandle* window)
        {
            _window = window;
        }

        public override unsafe void Init()
        {
            loader = glfw.GetProcAddress;

            _gl = GL.GetApi(loader);
            
            MakeCurrent();
            
            GLExtensions.Load();

#if DEBUG
            Debugging.GLDebugging.Enable();
#endif
        }

        public override void MakeCurrent()
        {
            glfw.MakeContextCurrent(_window);
            gl = _gl;
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
