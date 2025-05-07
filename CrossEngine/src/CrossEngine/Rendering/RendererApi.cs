using System;
using System.Diagnostics;
using System.Numerics;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using CrossEngine.Logging;

using CrossEngine.Platform.OpenGL;
#if WINDOWS
using CrossEngine.Platform.Windows;
#endif

// todo: manual dispose object inheritance
namespace CrossEngine.Rendering
{
    class DummyRendererApi : RendererApi
    {
        public override void Clear() { }

        public override void DrawArray(WeakReference<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles) { }
        public override void DrawIndexed(WeakReference<VertexArray> vertexArray, uint indexCount = 0) { }

        public override void Init() { }

        public override void SetPolygonMode(PolygonMode mode) { }
        public override void SetBlendFunc(BlendFunc func) { }
        public override void SetClearColor(float r, float g, float b, float a) { }
        public override void SetDepthFunc(DepthFunc func) { }
        public override void SetCullFace(CullFace face) { }
        public override void SetDepthMask(bool flag) { }

        public override void SetLineWidth(float width) { }

        public override void SetViewport(uint x, uint y, uint width, uint height) { }
    }

    public enum GraphicsApi
    {
        None = 0,
        OpenGL,
        OpenGLES,
        GDI,
    }

    public abstract class RendererApi : IDisposable
    {
        internal static Logger Log = new Logger("rapi");

        private static GraphicsApi _api;

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static GraphicsApi GetApi() => _api;

        public static RendererApi Create(GraphicsApi api)
        {
            _api = api;
            switch (_api)
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new GLRendererApi();
#if WINDOWS
                case GraphicsApi.GDI: return new GdiRendererApi();
#endif
            }

            Debug.Assert(false, $"Unknown {nameof(GraphicsApi)} value");
            return null;
        }

        public abstract void Init();

        public abstract void SetViewport(uint x, uint y, uint width, uint height);
        public void SetClearColor(Vector4 col) => SetClearColor(col.X, col.Y, col.Z, col.W);
        public abstract void SetClearColor(float r, float g, float b, float a);
        public abstract void SetPolygonMode(PolygonMode mode);
        public abstract void SetDepthFunc(DepthFunc func);
        public abstract void SetBlendFunc(BlendFunc func);
        public abstract void SetCullFace(CullFace face);
        public abstract void SetDepthMask(bool flag);

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
    
    public enum CullFace
    {
        None = 0,

        Front,
        Back,
        
        Default = Front,
    }
}
