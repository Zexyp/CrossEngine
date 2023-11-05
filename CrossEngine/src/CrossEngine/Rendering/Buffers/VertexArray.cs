using System;
using System.Collections.Generic;
using System.Diagnostics;

#if WINDOWS
using CrossEngine.Platform.OpenGL;
#endif

namespace CrossEngine.Rendering.Buffers
{
    public abstract class VertexArray : IDisposable
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

        ~VertexArray()
        {
            Dispose(false);
        }

        public abstract void Bind();
        public abstract void Unbind();

        public abstract void AddVertexBuffer(WeakReference<VertexBuffer> vertexBuffer);
        public abstract void SetIndexBuffer(WeakReference<IndexBuffer> indexBuffer);

        public abstract List<WeakReference<VertexBuffer>> GetVertexBuffers();
        public abstract WeakReference<IndexBuffer> GetIndexBuffer();

        public static WeakReference<VertexArray> Create()
        {
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
#if WINDOWS
                case RendererAPI.API.OpenGL: return new WeakReference<VertexArray>(new GLVertexArray());
#endif
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }
}
