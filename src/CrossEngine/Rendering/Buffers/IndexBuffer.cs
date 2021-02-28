using static OpenGL.GL;

namespace CrossEngine.Rendering.Buffers
{
    public class IndexBuffer
    {
        uint id = 0;
        int count = 0;

        bool dynamic = false;

        public unsafe IndexBuffer(uint* data, int count, bool dynamic = false)
        {
            this.count = count;
            this.dynamic = dynamic;

            fixed (uint* idp = &id)
                glGenBuffers(1, idp);

            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, id);

            if (!dynamic)
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, count * sizeof(uint), data, GL_STATIC_DRAW);
            else
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, count * sizeof(uint), data, GL_DYNAMIC_DRAW);
        }

        public unsafe void Dispose()
        {
            fixed (uint* idp = &id)
                glDeleteBuffers(1, idp);
            id = 0;
        }

        public void Bind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, id);
        }

        public static void Unbind()
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        public unsafe void SetData(uint* data, int count, int offset = 0)
        {
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, id);

            if (!dynamic)
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, count * sizeof(uint), data, GL_STATIC_DRAW);
            else
                glBufferSubData(GL_ELEMENT_ARRAY_BUFFER, offset, count * sizeof(uint), data);
        }

        public int GetCount()
        {
            return count;
        }
    }
}
