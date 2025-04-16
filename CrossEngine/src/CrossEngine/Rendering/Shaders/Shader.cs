using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Logging;
using CrossEngine.Profiling;

using CrossEngine.Platform.OpenGL;
using CrossEngine.Platform.Windows;

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

    public abstract class Shader : IDisposable
    {
        public ShaderType Type { get; private set; }

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

            Disposed = true;
        }

        ~Shader()
        {
            Dispose(false);
        }

        public Shader(ShaderType type)
        {
            Type = type;
        }

        public static WeakReference<Shader> Create(string source, ShaderType type)
        {
            return Create(new WeakReference<Shader>(null), source, type);
        }
        
        public static WeakReference<Shader> Create(WeakReference<Shader> wr, string source, ShaderType type)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: wr.SetTarget(new GLShader(source, type)); return wr;
                case GraphicsApi.GDI: wr.SetTarget(new GdiShader(source, type)); return wr;
            }

            Debug.Assert(false, $"Udefined {nameof(GraphicsApi)} value");
            return null;
        }
    }
}
