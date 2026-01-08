using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

using CrossEngine.Utils;
using CrossEngine.Rendering.Shaders;

using CrossEngine.Platform.OpenGL;
#if WINDOWS
using CrossEngine.Platform.Windows;
#endif

namespace CrossEngine.Rendering.Buffers
{
    public enum IndexDataType
    {
        None = 0,

        //Int,
        //Short,
        //Byte,
        UInt,
        UShort,
        UByte,
    }

    public abstract class IndexBuffer : GpuObject
    {
        public IndexDataType DataType { get; protected set; }
        public uint Count { get; protected set; }

        public abstract void Bind();
        public abstract void Unbind();

        public abstract unsafe void SetData(void* data, uint count, uint offset = 0);

        public static unsafe IndexBuffer Create(void* indices, uint count, IndexDataType dataType, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new GLIndexBuffer(indices, count, dataType, bufferUsage);
#if WINDOWS
                case GraphicsApi.GDI: return new GdiIndexBuffer(indices, count, dataType);
#endif
            }

            Debug.Assert(false, $"Undefined {nameof(GraphicsApi)} value");
            return null;
        }
    }
}
