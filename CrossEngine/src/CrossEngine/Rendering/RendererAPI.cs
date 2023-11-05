using System;
using System.Diagnostics;
using System.Numerics;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using CrossEngine.Logging;

#if WINDOWS
using CrossEngine.Platform.OpenGL;
#endif

namespace CrossEngine.Rendering
{
    class DummyRendererAPI : RendererAPI
    {
        public override void Clear()
        {
        }

        public override void DrawArray(WeakReference<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles)
        {
        }

        public override void DrawIndexed(WeakReference<VertexArray> vertexArray, uint indexCount = 0)
        {
        }

        public override void Init()
        {
        }

        public override void SetBlendFunc(BlendFunc func)
        {
        }

        public override void SetClearColor(Vector4 color)
        {
        }

        public override void SetDepthFunc(DepthFunc func)
        {
        }

        public override void SetLineWidth(float width)
        {
        }

        public override void SetPolygonMode(PolygonMode mode)
        {
        }

        public override void SetViewport(uint x, uint y, uint width, uint height)
        {
        }
    }

    public abstract class RendererAPI : IDisposable
    {
        internal static Logger Log = new Logger("rapi");

        public enum API
        {
            None = 0,
            OpenGL,
            OpenGLES,
        }

        private static API _api;

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static API GetAPI() => _api;

        public static RendererAPI Create(API api)
        {
            _api = api;
            switch (_api)
            {
                case API.None: Debug.Assert(false, $"No API is not supported"); return null;
#if WINDOWS
                case API.OpenGL: return new GLRendererAPI();
#elif WASM
                case API.OpenGLES: return new DummyRendererAPI();
#endif
            }

            Debug.Assert(false, $"Unknown {nameof(API)} value");
            return null;
        }

        public abstract void Init();

        public abstract void SetViewport(uint x, uint y, uint width, uint height);
        public abstract void SetClearColor(Vector4 color);
        public abstract void SetPolygonMode(PolygonMode mode);
        public abstract void SetDepthFunc(DepthFunc func);
        public abstract void SetBlendFunc(BlendFunc func);

        public abstract void SetLineWidth(float width);

        public abstract void Clear();

        public abstract void DrawIndexed(WeakReference<VertexArray> vertexArray, uint indexCount = 0/*, DrawMode mode*/);
        public abstract void DrawArray(WeakReference<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles);
    }

    public enum PolygonMode
    {
        None = default,

        Fill,
        Line,
        Point,
    }

    public enum DrawMode
    {
        None = default,

        Lines,
        Traingles,
        Points,
    }

    public enum BlendFunc
    {
        None = default,

        OneMinusSrcAlpha,
        One,
    }

    public enum DepthFunc
    {
        None = 0,

        Never,
        Less,
        Equal,
        LessEqual,
        Greater,
        NotEqual,
        GreaterEqual,
        Always,

        Default = Less,
    }
}
