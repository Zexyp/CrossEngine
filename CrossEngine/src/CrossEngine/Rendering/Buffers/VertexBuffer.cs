using System;
using System.Diagnostics;

using CrossEngine.Platform.OpenGL;
#if WINDOWS
using CrossEngine.Platform.Windows;
#endif

namespace CrossEngine.Rendering.Buffers
{
    public abstract class VertexBuffer : IDisposable
    {
        public bool Disposed { get; protected set; } = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                // free any other managed objects here
            }

            // free any unmanaged objects here

            Disposed = true;
        }

        ~VertexBuffer()
        {
            Dispose(false);
        }

        public abstract void Bind();
        public abstract void Unbind();

        public abstract void SetLayout(BufferLayout layout);
        public abstract BufferLayout GetLayout();

        public abstract unsafe void SetData(void* data, uint size, uint offset = 0);

        public static unsafe WeakReference<VertexBuffer> Create(void* vertices, uint size, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            return Create(new WeakReference<VertexBuffer>(null), vertices, size, bufferUsage);
        }

        public static unsafe WeakReference<VertexBuffer> Create(WeakReference<VertexBuffer> wr, void* vertices, uint size, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: wr.SetTarget(new GLVertexBuffer(vertices, size, bufferUsage)); return wr;
#if WINDOWS
                case GraphicsApi.GDI: wr.SetTarget(new GdiVertexBuffer(vertices, size)); return wr;
#endif
            }

            Debug.Assert(false, $"Udefined {nameof(GraphicsApi)} value");
            return null;
        }
    }

}
