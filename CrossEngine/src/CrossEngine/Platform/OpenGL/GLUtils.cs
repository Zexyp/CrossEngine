using static OpenGL.GL;

using System.Diagnostics;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Textures;
using System;

namespace CrossEngine.Platform.OpenGL
{
    static class GLUtils
    {
        public static int ToGLBufferUsage(BufferUsageHint bufferUsage)
        {
            switch (bufferUsage)
            {
                case BufferUsageHint.StaticDraw: return GL_STATIC_DRAW;
                case BufferUsageHint.DynamicDraw: return GL_DYNAMIC_DRAW;
                case BufferUsageHint.StreamDraw: return GL_STREAM_DRAW;
            }

            Debug.Assert(false, $"Unknown {nameof(BufferUsageHint)} value");
            return 0;
        }

        public static int ToGLIndexDataType(IndexDataType indexType)
        {
            switch (indexType)
            {
                case IndexDataType.UInt: return GL_UNSIGNED_INT;
                case IndexDataType.UShort: return GL_UNSIGNED_SHORT;
                case IndexDataType.UByte: return GL_UNSIGNED_BYTE;
            }

            Debug.Assert(false, $"Unknown {nameof(IndexDataType)} value");
            return 0;
        }

        public static int GetGLBaseDataType(ShaderDataType dataType)
        {
            switch (dataType)
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

            Debug.Assert(false, $"Unknown {nameof(ShaderDataType)} value");
            return 0;
        }

        public static int ToGLDrawMode(DrawMode mode)
        {
            switch (mode)
            {
                case DrawMode.Lines: return GL_LINES;
                case DrawMode.Traingles: return GL_TRIANGLES;
                case DrawMode.Points: return GL_POINTS;
            }

            Debug.Assert(false, $"Unknown {nameof(DrawMode)} value");
            return 0;
        }

        public static int ToGLFilterParameter(FilterParameter filter)
        {
            switch (filter)
            {
                case FilterParameter.Linear: return GL_LINEAR;
                case FilterParameter.Nearest: return GL_NEAREST;
            }

            Debug.Assert(false, $"Unknown {nameof(FilterParameter)} value");
            return 0;
        }

        internal static int ToGLWrapParameter(WrapParameter param)
        {
            switch (param)
            {
                case WrapParameter.Repeat: return GL_REPEAT;
                case WrapParameter.MirroredRepeat: return GL_MIRRORED_REPEAT;
                case WrapParameter.ClampToEdge: return GL_CLAMP_TO_EDGE;
                case WrapParameter.ClampToBorder: return GL_CLAMP_TO_BORDER;
            }

            Debug.Assert(false, $"Unknown {nameof(WrapParameter)} value");
            return 0;
        }

        public static int ToGLTextureTarget(TextureTarget target)
        {
            switch (target)
            {
                case TextureTarget.Texture1D: return GL_TEXTURE_1D;
                case TextureTarget.Texture2D: return GL_TEXTURE_2D;
                case TextureTarget.Texture3D: return GL_TEXTURE_3D;
                case TextureTarget.TextureCubeMap: return GL_TEXTURE_CUBE_MAP;
            }

            Debug.Assert(false, $"Unknown {nameof(TextureTarget)} value");
            return 0;
        }

        public static int ToGLPolygonMode(PolygonMode mode)
        {
            switch (mode)
            {
                case PolygonMode.Fill: return GL_FILL;
                case PolygonMode.Line: return GL_LINE;
                case PolygonMode.Point: return GL_POINT;
            }

            Debug.Assert(false, $"Unknown {nameof(PolygonMode)} value");
            return 0;
        }

        public static int ToGLShaderType(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.Vertex: return GL_VERTEX_SHADER;
                case ShaderType.Geometry: return GL_GEOMETRY_SHADER;
                case ShaderType.Fragment: return GL_FRAGMENT_SHADER;
            }

            Debug.Assert(false, $"Unknown {nameof(ShaderType)} value");
            return 0;
        }

        public static ShaderDataType ToShaderDataType(int type)
        {
            switch (type)
            {
                case GL_FLOAT: return ShaderDataType.Float;
                case GL_FLOAT_VEC2: return ShaderDataType.Float2;
                case GL_FLOAT_VEC3: return ShaderDataType.Float3;
                case GL_FLOAT_VEC4: return ShaderDataType.Float4;

                case GL_FLOAT_MAT3: return ShaderDataType.Mat3;
                case GL_FLOAT_MAT4: return ShaderDataType.Mat4;

                case GL_INT: return ShaderDataType.Int;
                case GL_INT_VEC2: return ShaderDataType.Int2;
                case GL_INT_VEC3: return ShaderDataType.Int3;
                case GL_INT_VEC4: return ShaderDataType.Int4;

                case GL_BOOL: return ShaderDataType.Bool;

                case GL_SAMPLER_2D: return ShaderDataType.Sampler2D;
            }

            Debug.Assert(false, $"Invalid type or unknown {nameof(ShaderDataType)} value");
            return 0;
        }

        public static int ToGLColorFormat(ColorFormat format)
        {
            switch (format)
            {
                //case ColorFormat.SingleR: return GL_RED;
                //case ColorFormat.SingleG: throw new NotImplementedException();
                //case ColorFormat.SingleB: throw new NotImplementedException();
                //case ColorFormat.SingleA: throw new NotImplementedException();
                //case ColorFormat.DoubleRG: return GL_RG;
                case ColorFormat.RGB: return GL_RGB;
                case ColorFormat.BGR: return GL_BGR;
                case ColorFormat.RGBA: return GL_RGBA;
                case ColorFormat.BGRA: return GL_BGRA;
            }

            Debug.Assert(false, $"Unknown {nameof(ColorFormat)} value");
            return 0;
        }

        public static int ToGLTextureFormat(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.ColorRGBA8: return GL_RGB8;
                case TextureFormat.ColorR32I: return GL_R32I;
                case TextureFormat.ColorRGBA32F: return GL_RGBA32F;
                case TextureFormat.Depth24Stencil8: return GL_DEPTH24_STENCIL8;
            }

            Debug.Assert(false, $"Unknown {nameof(TextureFormat)} value");
            return 0;
        }
    }
}
