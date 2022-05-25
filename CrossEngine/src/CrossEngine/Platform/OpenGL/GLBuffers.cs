using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;

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
                glGenBuffers(1, p);
        }

        public unsafe GLVertexBuffer(void* vertices, uint size, BufferUsageHint bufferUsage = BufferUsageHint.StaticDraw) : this()
        {
            _bufferUsage = bufferUsage;

            glBindBuffer(GL_ARRAY_BUFFER, _rendererId);
            glBufferData(GL_ARRAY_BUFFER, (int)size, vertices, GLUtils.ToGLBufferUsage(_bufferUsage));
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

            Disposed = true;
        }

        public override void Bind()
        {
            Profiler.Function();

            glBindBuffer(GL_ARRAY_BUFFER, _rendererId);
        }

        public override void Unbind()
        {
            Profiler.Function();

            glBindBuffer(GL_ARRAY_BUFFER, 0);
        }

        public override void SetLayout(BufferLayout layout) => _layout = layout;
        public override BufferLayout GetLayout() => _layout;

        public override unsafe void SetData(void* data, uint size, uint offset = 0)
        {
            Profiler.Function();

            glBindBuffer(GL_ARRAY_BUFFER, _rendererId);
            glBufferSubData(GL_ARRAY_BUFFER, (int)offset, (int)size, data);
        }
    }

    class GLIndexBuffer : IndexBuffer
    {
        internal uint _rendererId;

        private BufferUsageHint _bufferUsage;

        unsafe GLIndexBuffer()
        {
            Profiler.Function();

            fixed (uint* p = &_rendererId)
                glGenBuffers(1, p);
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

    class GLVertexArray : VertexArray
    {
        internal uint _rendererId;
        private uint _vertexBufferIndex;
        private Ref<IndexBuffer> _indexBuffer;
        private List<VertexBuffer> _vertexBuffers = new List<VertexBuffer>();

        public unsafe GLVertexArray()
        {
            Profiler.Function();

            fixed (uint* p = &_rendererId)
                glGenVertexArrays(1, p);
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

        public override unsafe void AddVertexBuffer(Ref<VertexBuffer> vb)
        {
            Profiler.Function();

            Debug.Assert(((VertexBuffer)vb).GetLayout() != null && ((VertexBuffer)vb).GetLayout().GetElements().Length > 0);

            var vertexBuffer = ((VertexBuffer)vb);

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

        public override void SetIndexBuffer(Ref<IndexBuffer> indexBuffer)
        {
            Profiler.Function();

            glBindVertexArray(_rendererId);
            indexBuffer.Value.Bind();

            _indexBuffer = indexBuffer;
        }

        public override List<Ref<VertexBuffer>> GetVertexBuffers()
        {
            throw new NotImplementedException();
        }

        public override Ref<IndexBuffer> GetIndexBuffer()
        {
            return _indexBuffer;
        }
    }
}
