using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using CrossEngine.Utils;
using CrossEngine.Rendering.Shaders;

#if WINDOWS
using CrossEngine.Platform.OpenGL;
#endif

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
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
#if WINDOWS
                case RendererAPI.API.OpenGL: return new WeakReference<IndexBuffer>(new GLIndexBuffer(indices, count, dataType, bufferUsage));
#endif
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }
}
