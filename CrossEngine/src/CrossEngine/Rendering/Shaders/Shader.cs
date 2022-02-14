using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Logging;
using CrossEngine.Profiling;
using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering.Shaders
{
    public enum ShaderDataType
    {
        None = 0,

        Float,
        Float2,
        Float3,
        Float4,

        Mat3,
        Mat4,

        Int,
        Int2,
        Int3,
        Int4,

        Bool,

        Sampler2D,
    }

    public enum ShaderType
    {
        None = 0,

        Vertex,
        Geometry,
        Fragment,
    }

    public class Shader : IDisposable
    {
        uint _rendererId;
        public ShaderType Type;

        public uint RendererId => _rendererId;

        public bool Disposed { get; protected set; } = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Profiler.Function();

            if (Disposed)
                return;

            if (disposing)
            {
                // free any other managed objects here
            }

            // free any unmanaged objects here
            glDeleteShader(_rendererId);

            Disposed = true;
        }

        ~Shader()
        {
            Dispose(false);
        }

        public Shader(string source, ShaderType type)
        {
            Profiler.Function();

            Type = type;

            _rendererId = glCreateShader(GLUtils.ToGLShaderType(Type));
            glShaderSource(_rendererId, source);
            glCompileShader(_rendererId);

            CheckCompileErrors();
        }

        // true if error found
        private unsafe bool CheckCompileErrors()
        {
            int compiled = 0;
            glGetShaderiv(_rendererId, GL_COMPILE_STATUS, &compiled);
            if (compiled == GL_FALSE)
            {
                int length = 0;
                glGetShaderiv(_rendererId, GL_INFO_LOG_LENGTH, &length);

                char[] infoLog = new char[length];
                Log.Core.Error($"{Type} shader compilation failed!\n" + glGetShaderInfoLog(_rendererId, length));

                return true;
            }
            return false;
        }
    }
}
