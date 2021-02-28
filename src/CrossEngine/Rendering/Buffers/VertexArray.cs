using System.Collections.Generic;
using static OpenGL.GL;

namespace CrossEngine.Rendering.Buffers
{
    public class VertexArray
    {
        uint id = 0;

        public uint lastVertexAttribArray = 0;

        public unsafe VertexArray()
        {
            fixed (uint* idp = &id)
                glGenVertexArrays(1, idp);
        }

        //unsafe ~VertexArray()
        //{
        //    fixed (uint* idp = &id)
        //        glDeleteVertexArrays(1, idp);
        //}

        public unsafe void Dispose()
        {
            fixed (uint* idp = &id)
                glDeleteVertexArrays(1, idp);
            id = 0;
        }

        public unsafe void AddBuffer(VertexBuffer vb, VertexBufferLayout layout, uint attribOffset = 0)
        {
            glBindVertexArray(id);

            vb.Bind(); // this buffer will be assosiated with those attributes!
            
            VertexBufferElement[] elements = layout.GetElements();

            int offset = 0;
            for(uint i = 0; i < elements.Length; i++)
            {
                VertexBufferElement element = elements[(int)i];
                glEnableVertexAttribArray(i + attribOffset);
                glVertexAttribPointer(i + attribOffset, element.count, (int)element.type, element.normalized, layout.GetStride(), (void*)offset);
                glVertexAttribDivisor(i + attribOffset, element.divisor);

                //Log.Debug("stride: " + layout.GetStride() + "; offset: " + offset);

                offset += element.count * VertexBufferElement.GetSizeOfType(element.type);

                lastVertexAttribArray = i + attribOffset;
                //Log.Debug("added layout element:"
                //    + "\n    index: " + i
                //    + "\n    count: " + element.count
                //    + "\n    type: " + element.type
                //    + "\n    normalized: " + element.normalized);
            }

            glBindVertexArray(0);
        }

        public void Bind()
        {
            glBindVertexArray(id);
        }

        public static void Unbind()
        {
            glBindVertexArray(0);
        }
    }
}
