using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

using CrossEngine.Platform.OpenGL;
#if WINDOWS
using CrossEngine.Platform.Windows;
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

        public abstract void AddVertexBuffer(WasWeakReference<VertexBuffer> vertexBuffer);
        public abstract void SetIndexBuffer(WasWeakReference<IndexBuffer> indexBuffer);

        public abstract WasWeakReference<VertexBuffer>[] GetVertexBuffers();
        public abstract WasWeakReference<IndexBuffer> GetIndexBuffer();

        public static WasWeakReference<VertexArray> Create()
        {
            return Create(new WasWeakReference<VertexArray>(null));
        }

        public static WasWeakReference<VertexArray> Create(WeakReference<VertexArray> wr)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: wr.SetTarget(new GLVertexArray()); return wr;
#if WINDOWS
                case GraphicsApi.GDI: wr.SetTarget(new GdiVertexArray()); return wr;
#endif
            }

            Debug.Assert(false, $"Udefined {nameof(GraphicsApi)} value");
            return null;
        }
    }
}
