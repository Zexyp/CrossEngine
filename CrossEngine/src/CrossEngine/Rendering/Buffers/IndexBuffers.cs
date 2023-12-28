using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using CrossEngine.Utils;
using CrossEngine.Rendering.Shaders;

using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering.Buffers
{
    public enum IndexDataType
    {
        None = 0,

        UInt,
        UShort,
        UByte,
    }

    public abstract class IndexBuffer : IDisposable
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

        ~IndexBuffer()
        {
            Dispose(false);
        }

        public IndexDataType DataType { get; protected set; }
        public uint Count { get; protected set; }

        public abstract void Bind();
        public abstract void Unbind();

        public abstract unsafe void SetData(void* data, uint size, uint offset = 0);

        public static unsafe WeakReference<IndexBuffer> Create(void* indices, uint count, IndexDataType dataType, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new WeakReference<IndexBuffer>(new GLIndexBuffer(indices, count, dataType, bufferUsage));
            }

            Debug.Assert(false, $"Udefined {nameof(GraphicsApi)} value");
            return null;
        }
    }
}
