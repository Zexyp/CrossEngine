using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Logging;
using CrossEngine.Profiling;

using CrossEngine.Platform.OpenGL;
#if WINDOWS
using CrossEngine.Platform.Windows;
#endif

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
        SamplerCube,
    }

    public enum ShaderType
    {
        None = 0,

        Vertex,
        Geometry,
        Fragment,
    }

    public abstract class Shader : GpuObject
    {
        public ShaderType Type { get; private set; }

        public Shader(ShaderType type)
        {
            Type = type;
        }

        public static Shader Create(string source, ShaderType type)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new GLShader(source, type);
#if WINDOWS
                case GraphicsApi.GDI: wr.SetTarget(new GdiShader(source, type)); return wr;
#endif
            }

            Debug.Assert(false, $"Udefined {nameof(GraphicsApi)} value");
            return null;
        }
    }
}
