﻿using System;

using System.Collections.Generic;
using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Debugging;
using CrossEngine.Rendering;

#if WASM
using GLEnum = Silk.NET.OpenGLES.GLEnum;
using static CrossEngine.Platform.Wasm.EGLContext;
#else
using GLEnum = Silk.NET.OpenGL.GLEnum;
using static CrossEngine.Platform.OpenGL.GLContext;
#endif

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

            RendererApi.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        public unsafe GLIndexBuffer(void* indices, uint count, IndexDataType dataType, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw) : this()
        {
            DataType = dataType;
            Count = count;
            _bufferUsage = bufferUsage;

            gl.BindBuffer(GLEnum.ElementArrayBuffer, _rendererId);
            gl.BufferData(GLEnum.ElementArrayBuffer, Count * GetIndexDataTypeSize(DataType), indices, GLUtils.ToGLBufferUsage(_bufferUsage));
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

            RendererApi.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

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

            gl.BindBuffer(GLEnum.ElementArrayBuffer, _rendererId);
            gl.BufferSubData(GLEnum.ElementArrayBuffer, (int)offset, Count * GetIndexDataTypeSize(DataType), data);
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
