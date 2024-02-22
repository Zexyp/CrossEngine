using System;
using GLFW;
using Silk.NET.OpenGL;

using CrossEngine.Rendering;
using CrossEngine.Logging;

namespace CrossEngine.Platform.OpenGL
{
    internal class GLContext : GraphicsContext
    {
        Window _window;

        public static Silk.NET.OpenGL.GL gl;

        internal static Func<string, IntPtr> loader;

        public GLContext(Window window)
        {
            _window = window;
        }

        public override unsafe void Init()
        {
            Glfw.MakeContextCurrent(_window);
            loader = Glfw.GetProcAddress;


            gl = GL.GetApi(loader);
            GLExtensions.Load();

#if DEBUG
            Debugging.GLDebugging.Enable(LogLevel.Debug);
#endif
        }

        public override void SwapBuffers()
        {
            Glfw.SwapBuffers(_window);
        }

        public override void Shutdown()
        {
            
        }
    }
}
