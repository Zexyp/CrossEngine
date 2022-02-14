using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using CrossEngine.Utils;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering.Buffers
{
    public enum BufferUsageHint
    {
        None = 0,

        StaticDraw,
        DynamicDraw,
        StreamDraw,
    }

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

        public static unsafe Ref<VertexBuffer> Create(void* vertices, uint size, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case RendererAPI.API.OpenGL: return (Ref<VertexBuffer>)new GLVertexBuffer(vertices, size, BufferUsageHint.StaticDraw);
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }

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

        public static unsafe Ref<IndexBuffer> Create(void* indices, uint count, IndexDataType dataType, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case RendererAPI.API.OpenGL: return (Ref<IndexBuffer>)new GLIndexBuffer(indices, count, dataType, bufferUsage); ;
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }

    public abstract class VertexArray
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

        public abstract void AddVertexBuffer(Ref<VertexBuffer> vertexBuffer);
		public abstract void SetIndexBuffer(Ref<IndexBuffer> indexBuffer);

		public abstract List<Ref<VertexBuffer>> GetVertexBuffers();
        public abstract Ref<IndexBuffer> GetIndexBuffer();

        public static Ref<VertexArray> Create()
        {
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case RendererAPI.API.OpenGL: return (Ref<VertexArray>)new GLVertexArray();
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }
}
