using CrossEngine.Platform.OpenGL;

using System;
using System.Diagnostics;

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
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case RendererAPI.API.OpenGL: return new WeakReference<VertexBuffer>(new GLVertexBuffer(vertices, size, BufferUsageHint.StaticDraw));
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }

}
