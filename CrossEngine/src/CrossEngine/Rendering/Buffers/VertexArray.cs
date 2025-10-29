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
    public abstract class VertexArray : GpuObject
    {
        public abstract void Bind();
        public abstract void Unbind();

        public abstract void AddVertexBuffer(VertexBuffer vertexBuffer);
        public abstract void SetIndexBuffer(IndexBuffer indexBuffer);

        public abstract VertexBuffer[] GetVertexBuffers();
        public abstract IndexBuffer GetIndexBuffer();

        public static VertexArray Create()
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new GLVertexArray();
#if WINDOWS
                case GraphicsApi.GDI: return new GdiVertexArray();
#endif
            }

            Debug.Assert(false, $"Undefined {nameof(GraphicsApi)} value");
            return null;
        }
    }
}
