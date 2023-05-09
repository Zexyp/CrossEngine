using System;
using GLFW;
using static OpenGL.GL;

using CrossEngine.Rendering;

namespace CrossEngine.Platform.OpenGL
{
    internal class GLContext : GraphicsContext
    {
        Window _window;

        public GLContext(Window window)
        {
            _window = window;
        }

        public override void Init()
        {
            Glfw.MakeContextCurrent(_window);
            Import(Glfw.GetProcAddress);
        }

        public override void SwapBuffers()
        {
            Glfw.SwapBuffers(_window);
        }
    }
}
