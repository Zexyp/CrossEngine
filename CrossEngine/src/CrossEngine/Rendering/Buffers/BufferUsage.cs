using static OpenGL.GL;

namespace CrossEngine.Rendering.Buffers
{
    public enum BufferUsage : int
    {
        StaticDraw = GL_STATIC_DRAW,
        DynamicDraw = GL_DYNAMIC_DRAW,
        StreamDraw = GL_STREAM_DRAW
    }
}
