using System;
using System.Diagnostics;

using CrossEngine.Platform.OpenGL;
#if WINDOWS
using CrossEngine.Platform.Windows;
#endif

namespace CrossEngine.Rendering.Buffers
{
    public abstract class VertexBuffer : GpuObject
    {
        public abstract void Bind();
        public abstract void Unbind();

        public abstract void SetLayout(BufferLayout layout);
        public abstract BufferLayout GetLayout();

        public abstract unsafe void SetData(void* data, uint size, uint offset = 0);

        public static unsafe VertexBuffer Create(void* vertices, uint size, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new GLVertexBuffer(vertices, size, bufferUsage);
#if WINDOWS
                case GraphicsApi.GDI: wr.SetTarget(new GdiVertexBuffer(vertices, size)); return wr;
#endif
            }

            Debug.Assert(false, $"Undefined {nameof(GraphicsApi)} value");
            return null;
        }
    }

}
