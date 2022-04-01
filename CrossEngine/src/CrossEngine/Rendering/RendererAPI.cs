using System.Diagnostics;
using System.Numerics;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering
{
    public abstract class RendererAPI
    {
        public enum API
        {
            None = 0,
            OpenGL,
        }

        private static API _api = API.OpenGL;

        public static API GetAPI() => _api;

        public static RendererAPI Create()
        {
            switch (_api)
            {
                case API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case API.OpenGL: return new GLRendererAPI();
            }

            Debug.Assert(false, $"Udefined {nameof(API)} value");
            return null;
        }

        public abstract void Init();

        public abstract void SetViewport(uint x, uint y, uint width, uint height);
        public abstract void SetClearColor(Vector4 color);
        public abstract void SetPolygonMode(PolygonMode mode);
        public abstract void SetDepthFunc(DepthFunc func);

        public abstract void Clear();

        public abstract void DrawIndexed(Ref<VertexArray> vertexArray, uint indexCount = 0/*, DrawMode mode*/);
        public abstract void DrawArray(Ref<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles);
    }

    public enum PolygonMode
    {
        None = 0,

        Fill,
        Line,
        Point,
    }

    public enum DrawMode
    {
        None = 0,

        Lines,
        Traingles,
        Points,
    }

    public enum BlendFunc
    {
        None = 0,

        OneMinusSrcAlpha,
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
