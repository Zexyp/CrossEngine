using System;
using Silk.NET.OpenGL;

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
    class GLVertexBuffer : VertexBuffer
    {
        internal uint _rendererId;

        private BufferUsageHint _bufferUsage;

        private BufferLayout _layout;

        unsafe GLVertexBuffer()
        {
            Profiler.Function();

            fixed (uint* p = &_rendererId)
                gl.GenBuffers(1, p);

            GC.KeepAlive(this);
            GPUGC.Register(this);

            RendererApi.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        public unsafe GLVertexBuffer(void* vertices, uint size, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw) : this()
        {
            _bufferUsage = bufferUsage;

            gl.BindBuffer(GLEnum.ArrayBuffer, _rendererId);
            gl.BufferData(GLEnum.ArrayBuffer, size, vertices, GLUtils.ToGLBufferUsage(_bufferUsage));
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

            gl.BindBuffer(GLEnum.ArrayBuffer, _rendererId);
        }

        public override void Unbind()
        {
            Profiler.Function();

            gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        }

        public override void SetLayout(BufferLayout layout) => _layout = layout;
        public override BufferLayout GetLayout() => _layout;

        public override unsafe void SetData(void* data, uint size, uint offset = 0)
        {
            Profiler.Function();

            gl.BindBuffer(GLEnum.ArrayBuffer, _rendererId);
            gl.BufferSubData(GLEnum.ArrayBuffer, (int)offset, size, data);
        }
    }
}
