using static OpenGL.GL;
using System;

using System.Collections.Generic;

using CrossEngine.Logging;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Assets.GC;

namespace CrossEngine.Rendering.Buffers
{
    public class VertexArray
    {
        uint id = 0;

        List<VertexBuffer> vertexBuffers = new List<VertexBuffer> { };
        IndexBuffer indexBuffer;

        public uint lastVertexAttribArray = 0;

        uint vertexBufferIndex = 0;

        public unsafe VertexArray()
        {
            fixed (uint* idp = &id)
                glGenVertexArrays(1, idp);

            Log.Core.Trace("generated vertex array (id: {0})", id);
        }

        #region Disposure
        ~VertexArray()
        {
            Log.Core.Warn("unhandled vertex array disposure (id: {0})", id);
            //System.Diagnostics.Debug.Assert(false);
            GPUGarbageCollector.MarkObject(GPUObjectType.VertexArray, id);
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
            if (id != 0)
            {
                Log.Core.Trace("deleting vertex array (id: {0})", id);

                fixed (uint* idp = &id)
                    glDeleteVertexArrays(1, idp);
                id = 0;
            }
        }
        #endregion

        // every buffer needs it's layout now!
        public unsafe void AddVertexBuffer(VertexBuffer vb)
        {
            if (vb.GetLayout() == null || vb.GetLayout().GetElements().Length == 0)
            {
                Log.Core.Error("vertex buffer has no layout");
                return;
            }

            glBindVertexArray(id);
            vb.Bind();

            BufferElement[] elements = vb.GetLayout().GetElements();
            BufferLayout layout = vb.GetLayout();

            // when giving type to attrib pointer we need the BASE type
            for (int i = 0; i < elements.Length; i++)
            {
                switch (elements[i].type)
                {
                    case ShaderDataType.Float:
                    case ShaderDataType.Float2:
                    case ShaderDataType.Float3:
                    case ShaderDataType.Float4:
                        {
                            glEnableVertexAttribArray(vertexBufferIndex);
                            glVertexAttribPointer(vertexBufferIndex, (int)elements[i].GetComponentCount(), BufferElement.ShaderDataTypeToBaseType(elements[i].type), elements[i].normalized, (int)layout.GetStride(), (void*)elements[i].offset);
                            
                            glVertexAttribDivisor(vertexBufferIndex, elements[i].divisor);
                            
                            vertexBufferIndex++;
                            break;
                        }

                    case ShaderDataType.Int:
                    case ShaderDataType.Int2:
                    case ShaderDataType.Int3:
                    case ShaderDataType.Int4:
                    case ShaderDataType.Bool:
                        {
                            glEnableVertexAttribArray(vertexBufferIndex);
                            // be aware of the I in the glVertexAttribIPointer
                            glVertexAttribIPointer(vertexBufferIndex, (int)elements[i].GetComponentCount(), BufferElement.ShaderDataTypeToBaseType(elements[i].type), (int)layout.GetStride(), (void*)elements[i].offset);

                            glVertexAttribDivisor(vertexBufferIndex, elements[i].divisor);
                            
                            vertexBufferIndex++;
                            break;
                        }

                    case ShaderDataType.Mat3:
                    case ShaderDataType.Mat4:
                        {
                            uint count = elements[i].GetComponentCount();
                            for (uint c = 0; c < count; c++)
                            {
                                glEnableVertexAttribArray(vertexBufferIndex);
                                glVertexAttribPointer(vertexBufferIndex, (int)count, BufferElement.ShaderDataTypeToBaseType(elements[i].type), elements[i].normalized, (int)layout.GetStride(), (void*)(elements[i].offset + sizeof(float) * count * c));
                                
                                glVertexAttribDivisor(vertexBufferIndex, elements[i].divisor);
                                
                                vertexBufferIndex++;
                            }
                            break;
                        }

                    default: Log.Core.Error("unknow shader data type"); break;
                }
            }

            vertexBuffers.Add(vb);
        }

        public void SetIndexBuffer(IndexBuffer ib)
        {
            glBindVertexArray(id);
            ib.Bind();

            indexBuffer = ib;
        }

        //public VertexBuffer GetVertexBuffers()
        //{
        //    
        //}

        public IndexBuffer GetIndexBuffer() => indexBuffer;

        public void Bind()
        {
            glBindVertexArray(id);
        }

        public static void Unbind()
        {
            glBindVertexArray(0);
        }

        /*
        public unsafe void AddBuffer(VertexBuffer vb, BufferLayout layout, uint attribOffset = 0)
        {
            glBindVertexArray(id);

            vb.Bind(); // this buffer will be assosiated with those attributes!
            
            BufferElement[] elements = layout.GetElements();

            int offset = 0;
            for(uint i = 0; i < elements.Length; i++)
            {
                BufferElement element = elements[(int)i];
                glEnableVertexAttribArray(i + attribOffset);
                glVertexAttribPointer(i + attribOffset, element.count, (int)element.type, element.normalized, layout.GetStride(), (void*)offset);
                glVertexAttribDivisor(i + attribOffset, element.divisor);

                //Log.Debug("stride: " + layout.GetStride() + "; offset: " + offset);

                offset += element.count * BufferElement.GetSizeOfType(element.type);

                lastVertexAttribArray = i + attribOffset;
                //Log.Debug("added layout element:"
                //    + "\n    index: " + i
                //    + "\n    count: " + element.count
                //    + "\n    type: " + element.type
                //    + "\n    normalized: " + element.normalized);
            }

            glBindVertexArray(0);
        }
        */
    }
}
