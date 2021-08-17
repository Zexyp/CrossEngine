using System;
using static OpenGL.GL;

using CrossEngine.Logging;
using CrossEngine.Assets.GC;

namespace CrossEngine.Rendering.Buffers
{
    public class VertexBuffer : IDisposable
    {
        uint _id = 0;
        int _size;

        BufferUsage _usage;

        BufferLayout layout;

        public unsafe VertexBuffer(BufferUsage usage = BufferUsage.StaticDraw)
        {
            this._usage = usage;
            this._size = 0;
            fixed (uint* p = &_id)
                glGenBuffers(1, p);

            Log.Core.Trace("generated vertex buffer (id: {0})", _id);
        }

        public unsafe VertexBuffer(void* data, int size, BufferUsage usage = BufferUsage.StaticDraw) : this(usage)
        {
            SetData(data, size);
        }

        #region Disposure
        ~VertexBuffer()
        {
            Log.Core.Warn("unhandled vertex buffer disposure (id: {0})", _id);
            //System.Diagnostics.Debug.Assert(false);
            GPUGarbageCollector.MarkObject(GPUObjectType.VertexBuffer, _id);
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
                Log.Core.Trace("deleting vertex buffer (id: {0})", _id);

                fixed (uint* idp = &_id)
                    glDeleteBuffers(1, idp);
                _id = 0;
            }
        }
        #endregion

        public void Bind()
        {
            glBindBuffer(GL_ARRAY_BUFFER, _id);
        }

        public static void Unbind()
        {
            glBindBuffer(GL_ARRAY_BUFFER, 0);
        }

        public unsafe void SetData(void* data, int size, bool orphaning = false, int offset = 0, BufferUsage? usage = null)
        {
            this._size = size;

            if (usage != null) _usage = (BufferUsage)usage;

            glBindBuffer(GL_ARRAY_BUFFER, _id);

            if (orphaning)
            {
                glBufferData(GL_ARRAY_BUFFER, 0, null, (int)_usage); // buffer orphaning, a way to improve streaming performance
                glBufferSubData(GL_ARRAY_BUFFER, offset, _size, data);
            }
            else
            {
                glBufferData(GL_ARRAY_BUFFER, _size, data, (int)_usage);
            }
        }

        public void SetLayout(BufferLayout layout) => this.layout = layout;
        public BufferLayout GetLayout() => layout;
    }
}
