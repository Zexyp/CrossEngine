using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Debugging;
using CrossEngine.Rendering;

namespace CrossEngine.Platform.OpenGL
{
    class GLIndexBuffer : IndexBuffer
    {
        internal uint _rendererId;

        private BufferUsageHint _bufferUsage;

        unsafe GLIndexBuffer()
        {
            Profiler.Function();

            fixed (uint* p = &_rendererId)
                glGenBuffers(1, p);

            GC.KeepAlive(this);
            GPUGC.Register(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        public unsafe GLIndexBuffer(void* indices, uint count, IndexDataType dataType, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw) : this()
        {
            DataType = dataType;
            Count = count;
            _bufferUsage = bufferUsage;

            glBindBuffer(GL_ARRAY_BUFFER, _rendererId);
            glBufferData(GL_ARRAY_BUFFER, (int)Count * GetIndexDataTypeSize(DataType), indices, GLUtils.ToGLBufferUsage(_bufferUsage));
        }

        protected override unsafe void Dispose(bool disposing)
        {
            Profiler.Function();

            if (Disposed)
                return;

            if (disposing)
            {
                // free any other managed objects here
            }

            // free any unmanaged objects here
            fixed (uint* p = &_rendererId)
                glDeleteBuffers(1, p);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        public override void Bind()
        {
            Profiler.Function();

            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _rendererId);
        }

        public override void Unbind()
        {
            Profiler.Function();

            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public override unsafe void SetData(void* data, uint count, uint offset = 0)
        {
            Profiler.Function();

            glBindBuffer(GL_ARRAY_BUFFER, _rendererId);
            glBufferSubData(GL_ARRAY_BUFFER, (int)offset, (int)Count * GetIndexDataTypeSize(DataType), data);
        }

        private static int GetIndexDataTypeSize(IndexDataType dataType)
        {
            switch (dataType)
            {
                case IndexDataType.UInt: return 4;
                case IndexDataType.UShort: return 2;
                case IndexDataType.UByte: return 1;
            }

            Debug.Assert(false, $"Unknown {nameof(IndexDataType)} value");
            return 0;
        }
    }
}
