using System.Diagnostics;

using CrossEngine.Rendering.Shaders;

namespace CrossEngine.Rendering.Buffers
{
    public struct BufferElement
    {
        public string Name;
        public ShaderDataType Type;
        public uint Size;
        public uint Offset;
        public bool Normalized;
        public uint Divisor;

        public BufferElement(ShaderDataType type, string name, bool normalized = false, uint divisor = 0)
        {
            this.Name = name;
            this.Type = type;
            this.Size = ShaderDataTypeSize(type);
            this.Offset = 0;
            this.Normalized = normalized;
            this.Divisor = divisor;
        }

        public uint GetComponentCount()
        {
            switch (Type)
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

            Debug.Assert(false, $"Unknown {nameof(ShaderDataType)} value");
            return 0;
        }

        public static uint ShaderDataTypeSize(ShaderDataType type)
        {
            switch (type)
            {
                case ShaderDataType.Float: return sizeof(float);
                case ShaderDataType.Float2: return sizeof(float) * 2;
                case ShaderDataType.Float3: return sizeof(float) * 3;
                case ShaderDataType.Float4: return sizeof(float) * 4;

                case ShaderDataType.Mat3: return sizeof(float) * 3 * 3;
                case ShaderDataType.Mat4: return sizeof(float) * 4 * 4;

                case ShaderDataType.Int: return sizeof(int);
                case ShaderDataType.Int2: return sizeof(int) * 2;
                case ShaderDataType.Int3: return sizeof(int) * 3;
                case ShaderDataType.Int4: return sizeof(int) * 4;

                case ShaderDataType.Bool: return sizeof(bool);
            }

            Debug.Assert(false, $"Unknown {nameof(ShaderDataType)} value");
            return 0;
        }
    }

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
                elements[i].Offset = offset;
                offset += elements[i].Size;
                stride += elements[i].Size;
            }
        }

        public uint GetStride() => stride;
        public BufferElement[] GetElements() => elements;
    }
}
