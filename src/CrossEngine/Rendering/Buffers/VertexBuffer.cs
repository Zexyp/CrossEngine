using System;
using static OpenGL.GL;

namespace CrossEngine.Rendering.Buffers
{
    public class VertexBuffer
    {
        uint id = 0;
        int size = 0;

        bool dynamic = false;

        public unsafe VertexBuffer(void* data, int size, bool dynamic = false)
        {
            this.size = size;
            this.dynamic = dynamic;

            fixed (uint* idp = &id)
                glGenBuffers(1, idp);

            glBindBuffer(GL_ARRAY_BUFFER, id);

            if (!dynamic)
                glBufferData(GL_ARRAY_BUFFER, size, data, GL_STATIC_DRAW);
            else
                glBufferData(GL_ARRAY_BUFFER, size, data, GL_DYNAMIC_DRAW);
        }

        public unsafe void Dispose()
        {
            fixed (uint* idp = &id)
                glDeleteBuffers(1, idp);
            id = 0;
        }

        public void Bind()
        {
            glBindBuffer(GL_ARRAY_BUFFER, id);
        }

        public static void Unbind()
        {
            glBindBuffer(GL_ARRAY_BUFFER, 0);
        }

        public unsafe void SetData(void* data, int size, int offset = 0)
        {
            glBindBuffer(GL_ARRAY_BUFFER, id);

            if (!dynamic)
                glBufferData(GL_ARRAY_BUFFER, size, data, GL_STATIC_DRAW);
            else
                glBufferSubData(GL_ARRAY_BUFFER, offset, size, data);
        }
    }
}
