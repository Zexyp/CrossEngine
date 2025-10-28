using System;
using System.Diagnostics;

using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering.Buffers
{
    [Obsolete("not implemented yet")]
    public abstract class UniformBuffer : GpuObject
    {
        public abstract void Bind();
        public abstract void Unbind();

        public abstract unsafe void SetData(void* data, uint size, uint offset = 0);

        public static unsafe VertexBuffer Create(void* vertices, uint size, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: throw new NotImplementedException(); //wr.SetTarget(new GLUniformBuffer(vertices, size, bufferUsage)); return wr;
            }

            Debug.Assert(false, $"Undefined {nameof(GraphicsApi)} value");
            return null;
        }
    }

}
