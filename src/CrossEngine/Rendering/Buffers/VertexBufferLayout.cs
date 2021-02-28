using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Numerics;

namespace CrossEngine.Rendering.Buffers
{
    public class VertexBufferElement
    {
        public VertexBufferElementType type;
        public int count;
        public bool normalized;
        public uint divisor;

        public VertexBufferElement(VertexBufferElementType type, int count, bool normalized)
        {
            this.type = type;
            this.count = count;
            this.normalized = normalized;
            this.divisor = 0;
        }

        public static int GetSizeOfType(VertexBufferElementType type)
        {
            switch(type)
            {
                case (VertexBufferElementType)GL_FLOAT: return 4;
            }

            Log.Warn("another data type in vertex buffer layout");
            Log.Error("wrong data type in vertex buffer layout!");

            return 0;
        }
    }

    public enum VertexBufferElementType
    {
        Float = GL_FLOAT,
    }

    public class VertexBufferLayout
    {
        // types:
        //float
        //byte
        //short

        public List<VertexBufferElement> elements = new List<VertexBufferElement> { };
        int stride = 0;

        Dictionary<Type, Func<VertexBufferElement>> typeSwitch;

        public unsafe VertexBufferLayout()
        {
            typeSwitch = new Dictionary<Type, Func<VertexBufferElement>> {
                    { typeof(float), () => { stride += sizeof(float); return new VertexBufferElement(VertexBufferElementType.Float, 1, false); } },
                    { typeof(Vector2), () => { stride += sizeof(Vector2); return new VertexBufferElement(VertexBufferElementType.Float, 2, false);; } },
                    { typeof(Vector3), () => { stride += sizeof(Vector3); return new VertexBufferElement(VertexBufferElementType.Float, 3, false);; } },
                    { typeof(Vector4), () => { stride += sizeof(Vector4); return new VertexBufferElement(VertexBufferElementType.Float, 4, false);; } }
                };
        }

        //public void RawAdd(int gltype, int count)
        //{
        //    if (!Enum.IsDefined(typeof(ElementType), gltype))
        //        Log.Warn("invalid gl type given to vertex buffer layout");
        //
        //    stride += count * VertexBufferElement.GetSizeOfType((ElementType)gltype);
        //
        //    elements.Add(new VertexBufferElement((ElementType)gltype, count, false));
        //}

        public void RawAdd(VertexBufferElementType gltype, int count)
        {
            stride += count * VertexBufferElement.GetSizeOfType(gltype);

            elements.Add(new VertexBufferElement(gltype, count, false));
        }

        public void Add(Type structure) // creates layout from structure type
        {
            if (!structure.IsValueType)
                throw new Exception("invalid type given to vertex buffer layout!");

            if(typeSwitch.ContainsKey(structure)) // check for vectors (because they have floats)
            {
                elements.Add(typeSwitch[structure]());
                return;
            }

            System.Reflection.FieldInfo[] fields = structure.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].FieldType.IsValueType) // checks the field
                    throw new Exception("type given to vertex buffer layout is containing non value type!");
                if(typeSwitch.ContainsKey(fields[i].FieldType)) // check if it can be added
                {
                    elements.Add(typeSwitch[fields[i].FieldType]()); // invokes the appropriate VertexBufferElement construction
                }
                else // othervise logs error
                {
                    Log.Warn("unknown data type given to vertex buffer layout");
                }
            }
        }

        public VertexBufferElement[] GetElements()
        {
            return elements.ToArray();
        }

        public int GetStride()
        {
            return stride;
        }
    }
}
