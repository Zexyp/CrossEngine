﻿using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Debugging;
using CrossEngine.Rendering;
using CrossEngine.Utils;

namespace CrossEngine.Platform.OpenGL
{
    class GLVertexArray : VertexArray
    {
        internal uint _rendererId;
        private uint _vertexBufferIndex;
        private WeakReference<IndexBuffer> _indexBuffer;
        private List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();

        public unsafe GLVertexArray()
        {
            Profiler.Function();

            fixed (uint* p = &_rendererId)
                glGenVertexArrays(1, p);

            GC.KeepAlive(this);
            GPUGC.Register(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
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
                glDeleteVertexArrays(1, p);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        public override void Bind()
        {
            Profiler.Function();

            glBindVertexArray(_rendererId);
        }

        public override void Unbind()
        {
            Profiler.Function();

            glBindVertexArray(0);
        }

        public override unsafe void AddVertexBuffer(WeakReference<VertexBuffer> vb)
        {
            Profiler.Function();

            var vertexBuffer = vb.GetValue();

            Debug.Assert(vertexBuffer.GetLayout() != null && vertexBuffer.GetLayout().GetElements().Length > 0);

            glBindVertexArray(_rendererId);
            vertexBuffer.Bind();

            var layout = vertexBuffer.GetLayout();
            var elements = layout.GetElements();
            for (int i = 0; i < elements.Length; i++)
            {
                var element = elements[i];
                switch (element.Type)
                {
                    case ShaderDataType.Float:
                    case ShaderDataType.Float2:
                    case ShaderDataType.Float3:
                    case ShaderDataType.Float4:
                        {
                            glEnableVertexAttribArray(_vertexBufferIndex);
                            glVertexAttribPointer(_vertexBufferIndex,
                                (int)element.GetComponentCount(),
                                GLUtils.GetGLBaseDataType(element.Type),
                                element.Normalized,
                                (int)layout.GetStride(),
                                (void*)element.Offset);
                            glVertexAttribDivisor(_vertexBufferIndex, element.Divisor);
                            _vertexBufferIndex++;
                            break;
                        }
                    case ShaderDataType.Int:
                    case ShaderDataType.Int2:
                    case ShaderDataType.Int3:
                    case ShaderDataType.Int4:
                    case ShaderDataType.Bool:
                        {
                            glEnableVertexAttribArray(_vertexBufferIndex);
                            glVertexAttribIPointer(_vertexBufferIndex,
                                (int)element.GetComponentCount(),
                                GLUtils.GetGLBaseDataType(element.Type),
                                (int)layout.GetStride(),
                                (void*)element.Offset);
                            glVertexAttribDivisor(_vertexBufferIndex, element.Divisor);
                            _vertexBufferIndex++;
                            break;
                        }
                    case ShaderDataType.Mat3:
                    case ShaderDataType.Mat4:
                        {
                            int count = (int)element.GetComponentCount();
                            for (int ii = 0; ii < count; ii++)
                            {
                                glEnableVertexAttribArray(_vertexBufferIndex);
                                glVertexAttribPointer(_vertexBufferIndex,
                                    count,
                                    GLUtils.GetGLBaseDataType(element.Type),
                                    element.Normalized,
                                    (int)layout.GetStride(),
                                    (void*)(element.Offset + sizeof(float) * count * ii));
                                glVertexAttribDivisor(_vertexBufferIndex, element.Divisor);
                                _vertexBufferIndex++;
                            }
                            break;
                        }
                    default: Debug.Assert(false, $"Unknown {nameof(ShaderDataType)} value"); break;
                }
            }

            _vertexBuffers.Add(vertexBuffer);
        }

        public override void SetIndexBuffer(WeakReference<IndexBuffer> indexBuffer)
        {
            Profiler.Function();

            glBindVertexArray(_rendererId);
            indexBuffer.GetValue().Bind();

            _indexBuffer = indexBuffer;
        }

        public override List<WeakReference<VertexBuffer>> GetVertexBuffers()
        {
            throw new NotImplementedException();
        }

        public override WeakReference<IndexBuffer> GetIndexBuffer()
        {
            return _indexBuffer;
        }
    }
}