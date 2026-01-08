using System;
using System.Runtime.InteropServices;


#if WASM
using GLEnum = Silk.NET.OpenGLES.GLEnum;
using static CrossEngine.Platform.Wasm.EGLContext;
#else
using GLEnum = Silk.NET.OpenGL.GLEnum;
using static CrossEngine.Platform.OpenGL.GLContext;
#endif

namespace CrossEngine.Platform.OpenGL
{
    static class GLExtensions
    {
        static string[] _extensions;

        public static unsafe void Load()
        {
            var num = gl.GetInteger(GLEnum.NumExtensions);
            _extensions = new string[num];
            for (int i = 0; i < num; i++)
            {
                _extensions[i] = Marshal.PtrToStringUTF8((IntPtr)gl.GetString(GLEnum.Extensions, (uint)i));
            }
        }

        public static bool CheckExtension(string name)
        {
            for (int i = 0; i < _extensions.Length; i++)
            {
                if (_extensions[i] == name) return true;
            }
            throw new PlatformNotSupportedException($"OpenGL extension '{name}' not supported.");
            return false;
        }
    }
}
