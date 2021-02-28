using System;
using static OpenGL.GL;

namespace CrossEngine.Rendering.Buffers
{
    public class UniformBuffer
    {
        uint id = 0;
        int size = 0;

        bool dynamic = false;

        public unsafe UniformBuffer(void* data, int size, bool dynamic = false)
        {
            this.size = size;
            this.dynamic = dynamic;

            fixed (uint* idp = &id)
                glGenBuffers(1, idp);

            glBindBuffer(GL_UNIFORM_BUFFER, id);

            if (!dynamic)
                glBufferData(GL_UNIFORM_BUFFER, size, data, GL_STATIC_DRAW);
            else
                glBufferData(GL_UNIFORM_BUFFER, size, data, GL_DYNAMIC_DRAW);
        }

        public unsafe void Dispose()
        {
            fixed (uint* idp = &id)
                glDeleteBuffers(1, idp);
            id = 0;
        }

        public void Bind()
        {
            glBindBuffer(GL_UNIFORM_BUFFER, id);
        }

        public static void Unbind()
        {
            glBindBuffer(GL_UNIFORM_BUFFER, 0);
        }

        public unsafe void SetData(void* data, int size, int offset = 0)
        {
            glBindBuffer(GL_UNIFORM_BUFFER, id);

            if (!dynamic)
                glBufferData(GL_UNIFORM_BUFFER, size, data, GL_STATIC_DRAW);
            else
                glBufferSubData(GL_UNIFORM_BUFFER, offset, size, data);
        }

        public unsafe void BindTo(uint binding)
        {
            glBindBufferBase(GL_UNIFORM_BUFFER, binding, id);
        }
    }
}
