﻿using System;
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

            Log.Default.Trace("opengl context info:\nversion: {0}\nrenderer: {1}\nvendor: {2}",
                GLHelper.PtrToStringUtf8((IntPtr)gl.GetString(GLEnum.Version)),
                GLHelper.PtrToStringUtf8((IntPtr)gl.GetString(GLEnum.Renderer)),
                GLHelper.PtrToStringUtf8((IntPtr)gl.GetString(GLEnum.Vendor)));

#if DEBUG
            Debugging.GLDebugging.Enable(this);
#endif
        }

        public override void SwapBuffers()
        {
            Glfw.SwapBuffers(_window);
        }
    }
}
