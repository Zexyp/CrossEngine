using System;
using static OpenGL.GL;

using CrossEngine.Logging;
using CrossEngine.Assets.GC;

namespace CrossEngine.Rendering.Buffers
{
    public enum IndexDataType : int
    {
        UInt = GL_UNSIGNED_INT,
        UShort = GL_UNSIGNED_SHORT,
        UByte = GL_UNSIGNED_BYTE,
    }

    public class IndexBuffer : IDisposable
    {
        uint _id = 0;
        int _size;

        BufferUsage _usage;
        IndexDataType indexType = IndexDataType.UInt;

        public unsafe IndexBuffer(BufferUsage usage = BufferUsage.StaticDraw)
        {
            this._usage = usage;
            this._size = 0;
            fixed (uint* p = &_id)
                glGenBuffers(1, p);

            Log.Core.Trace("generated index buffer (id: {0})", _id);
        }

        public unsafe IndexBuffer(void* data, int size, BufferUsage usage = BufferUsage.StaticDraw) : this(usage)
        {
            SetData(data, size);
        }

        #region Disposure
        ~IndexBuffer()
        {
            Log.Core.Warn("unhandled index buffer disposure (id: {0})", _id);
            //System.Diagnostics.Debug.Assert(false);
            GPUGarbageCollector.MarkObject(GPUObjectType.IndexBuffer, _id);
            return;

            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (_id != 0)
            {
                Log.Core.Trace("deleting index buffer (id: {0})", _id);

                fixed (uint* idp = &_id)
                    glDeleteBuffers(1, idp);
                _id = 0;
            }
        }
        #endregion

        public void Bind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _id);
        }

        public static void Unbind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public unsafe void SetData(void* data, int size, bool orphaning = false, int offset = 0, BufferUsage? usage = null)
        {
            this._size = size;

            if (usage != null) _usage = (BufferUsage)usage;

            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, _id);

            if (orphaning)
            {
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, 0, null, (int)_usage); // buffer orphaning, a way to improve streaming performance
                glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, offset, _size, data);
            }
            else
            {
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, _size, data, (int)_usage);
            }
        }

        private uint IndexDataTypeSize(IndexDataType type)
        {
            switch (type)
            {
                case IndexDataType.UInt: return 4;
                case IndexDataType.UShort: return 2;
                case IndexDataType.UByte: return 1;
            }

            Log.Core.Error("unknown index data type given to buffer element");
            return 0;
        }

        public uint GetCount() => (uint)(_size / IndexDataTypeSize(indexType));
        public IndexDataType GetIndexDataType() => indexType;
    }
}
