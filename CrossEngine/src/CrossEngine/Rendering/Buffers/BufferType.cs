using static OpenGL.GL;

namespace CrossEngine.Rendering.Buffers
{
    public enum BufferUsage : int
    {
        StaticDraw = GL_STATIC_DRAW,
        DynamicDraw = GL_DYNAMIC_DRAW,
        StreamDraw = GL_STREAM_DRAW
    }

    //public enum BufferType : int
    //{
    //    Vertex = GL_ARRAY_BUFFER,
    //    Index = GL_ELEMENT_ARRAY_BUFFER,
    //    Uniform = GL_UNIFORM_BUFFER,
    //}
}
