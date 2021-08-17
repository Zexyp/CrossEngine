using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Numerics;

using CrossEngine.Logging;
using CrossEngine.Rendering.Shaders;

namespace CrossEngine.Rendering.Buffers
{
    #region Element
    public struct BufferElement
    {
        public string name;
        public ShaderDataType type;
        public uint size;
        public uint offset;
        public bool normalized;
        public uint divisor;

        public BufferElement(ShaderDataType type, string name, bool normalized = false, uint divisor = 0)
        {
            this.name = name;
            this.type = type;
            this.size = ShaderDataTypeSize(type);
            this.offset = 0;
            this.normalized = normalized;
            this.divisor = divisor;
        }

        public uint GetComponentCount()
		{
			switch (type)
			{
                case ShaderDataType.Float: return 1;
                case ShaderDataType.Float2: return 2;
                case ShaderDataType.Float3: return 3;
                case ShaderDataType.Float4: return 4;

                // they get handled on a differnt place while adding them to vertex array
                case ShaderDataType.Mat3: return 3; // 3 floats
                case ShaderDataType.Mat4: return 4; // 4 floats

                case ShaderDataType.Int: return 1;
                case ShaderDataType.Int2: return 2;
                case ShaderDataType.Int3: return 3;
                case ShaderDataType.Int4: return 4;

                case ShaderDataType.Bool: return 1;
            }

			Log.Core.Error("unknown shader data type");
			return 0;
		}

        private static uint ShaderDataTypeSize(ShaderDataType type)
        {
            switch (type)
            {
                case ShaderDataType.Float: return 4;
                case ShaderDataType.Float2: return 4 * 2;
                case ShaderDataType.Float3: return 4 * 3;
                case ShaderDataType.Float4: return 4 * 4;

                case ShaderDataType.Mat3: return 4 * 3 * 3;
                case ShaderDataType.Mat4: return 4 * 4 * 4;

                case ShaderDataType.Int: return 4;
                case ShaderDataType.Int2: return 4 * 2;
                case ShaderDataType.Int3: return 4 * 3;
                case ShaderDataType.Int4: return 4 * 4;

                case ShaderDataType.Bool: return 1;
            }

            Log.Core.Error("unknown shader data type given to buffer element");
            return 0;
        }

        // kinda useless
        public static int ShaderDataTypeToBaseType(ShaderDataType type)
        {
            switch (type)
            {
                case ShaderDataType.Float: return GL_FLOAT;
                case ShaderDataType.Float2: return GL_FLOAT;
                case ShaderDataType.Float3: return GL_FLOAT;
                case ShaderDataType.Float4: return GL_FLOAT;
                case ShaderDataType.Mat3: return GL_FLOAT;
                case ShaderDataType.Mat4: return GL_FLOAT;
                case ShaderDataType.Int: return GL_INT;
                case ShaderDataType.Int2: return GL_INT;
                case ShaderDataType.Int3: return GL_INT;
                case ShaderDataType.Int4: return GL_INT;
                case ShaderDataType.Bool: return GL_BOOL;
            }

            Log.Core.Error("unknown shader data type");
            return 0;
        }
    }
    #endregion

    public class BufferLayout
    {
        private BufferElement[] elements;
        private uint stride = 0;

        public BufferLayout(params BufferElement[] elements)
        {
            this.elements = elements;

            CalculateOffsetsAndStride();
        }

        private void CalculateOffsetsAndStride()
        {
            uint offset = 0;
            stride = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                elements[i].offset = offset;
                offset += elements[i].size;
                stride += elements[i].size;
            }
        }

        public uint GetStride() => stride;
        public BufferElement[] GetElements() => elements;
    }
}
