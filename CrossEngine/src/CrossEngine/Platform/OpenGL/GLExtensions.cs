using System;
using static OpenGL.GL;

namespace CrossEngine.Platform.OpenGL
{
    static class GLExtensions
    {
        static readonly string[] _extensions;
        static GLExtensions()
        {
            var num = glGetInteger(GL_NUM_EXTENSIONS);
            _extensions = new string[num];
            for (int i = 0; i < num; i++)
            {
                _extensions[i] = glGetStringi(GL_EXTENSIONS, (uint)i);
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
