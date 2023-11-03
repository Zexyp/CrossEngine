using System;

using System.Collections.Generic;
using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Debugging;
using CrossEngine.Rendering;
using static CrossEngine.Platform.OpenGL.GLContext;
using Silk.NET.OpenGL;

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
                gl.GenBuffers(1, p);

            GC.KeepAlive(this);
            GPUGC.Register(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        public unsafe GLIndexBuffer(void* indices, uint count, IndexDataType dataType, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw) : this()
        {
            DataType = dataType;
            Count = count;
            _bufferUsage = bufferUsage;

            gl.BindBuffer(GLEnum.ArrayBuffer, _rendererId);
            gl.BufferData(GLEnum.ArrayBuffer, Count * GetIndexDataTypeSize(DataType), indices, GLUtils.ToGLBufferUsage(_bufferUsage));
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
                gl.DeleteBuffers(1, p);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        public override void Bind()
        {
            Profiler.Function();

            gl.BindBuffer(GLEnum.ElementArrayBuffer, _rendererId);
        }

        public override void Unbind()
        {
            Profiler.Function();

            gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
        }

        public override unsafe void SetData(void* data, uint count, uint offset = 0)
        {
            Profiler.Function();

            gl.BindBuffer(GLEnum.ArrayBuffer, _rendererId);
            gl.BufferSubData(GLEnum.ArrayBuffer, (int)offset, Count * GetIndexDataTypeSize(DataType), data);
        }

        private static uint GetIndexDataTypeSize(IndexDataType dataType)
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
