using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using CrossEngine.Logging;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;

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
        public ReadOnlyCollection<BufferElement> Elements { get => _elements.AsReadOnly(); }
        public uint Stride { get; private set; }

        private BufferElement[] _elements;

        private readonly static IReadOnlyDictionary<Type, ShaderDataType> ShaderTypeSwitch = new Dictionary<Type, ShaderDataType>()
        {
            { typeof(float), ShaderDataType.Float },
            { typeof(Vector2), ShaderDataType.Float2 },
            { typeof(Vector3), ShaderDataType.Float3 },
            { typeof(Vector4), ShaderDataType.Float4 },

            { typeof(int), ShaderDataType.Int },
            { typeof(IntVec2), ShaderDataType.Int2 },
            { typeof(IntVec3), ShaderDataType.Int3 },
            { typeof(IntVec4), ShaderDataType.Int4 },

            { typeof(bool), ShaderDataType.Bool },
        }.AsReadOnly();

        private BufferLayout()
        {
            
        }
        
        public BufferLayout(params BufferElement[] elements)
        {
            this._elements = elements;
            
            CalculateOffsetsAndStride();
        }

        public static BufferLayout Manual(BufferElement[] elements, uint stride)
        {
            var layout = new BufferLayout();
            layout._elements = elements;
            layout.Stride = stride;
            return layout;
        }
        
        public static BufferLayout FromStructType<T>() where T : struct
        {
            return FromStructType(typeof(T));
        }
        
        public static BufferLayout FromStructType(Type structure)
        {
            BufferLayout layout = new BufferLayout();
            
            // check if type makes sense
            if (!structure.IsValueType)
                throw new InvalidOperationException("invalid type given to vertex buffer layout!");

            if (ShaderTypeSwitch.ContainsKey(structure)) // check for elements (because vectors have floats)
            {
                layout._elements = new[] {new BufferElement(ShaderTypeSwitch[structure], null)};
                layout.Stride = (uint)Marshal.SizeOf(structure);
                
                return layout;
            }

            List<BufferElement> elements = new List<BufferElement>();
            
            FieldInfo[] fields = structure.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].FieldType.IsValueType) // checks the field (ik it's already at the begining)
                    throw new InvalidOperationException("type given to vertex buffer layout is containing non value type!");

                if (ShaderTypeSwitch.ContainsKey(fields[i].FieldType)) // check if it can be added
                {
                    var element = new BufferElement(ShaderTypeSwitch[fields[i].FieldType], fields[i].Name);
                    element.Offset = (uint)Marshal.OffsetOf(structure, fields[i].Name);
                    elements.Add(element);
                }
                else // otherwise error
                {
                    throw new InvalidOperationException("unknown shader data type!");
                }
            }
            
            layout._elements = elements.ToArray();
            layout.Stride = (uint)Marshal.SizeOf(structure);
            
            return layout;
        }

        private void CalculateOffsetsAndStride()
        {
            uint offset = 0;
            Stride = 0;
            for (int i = 0; i < _elements.Length; i++)
            {
                _elements[i].Offset = offset;
                offset += _elements[i].Size;
                Stride += _elements[i].Size;
            }
        }
    }
}
