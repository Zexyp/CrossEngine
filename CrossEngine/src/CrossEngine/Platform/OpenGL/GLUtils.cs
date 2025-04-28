using System.Diagnostics;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Textures;
using System;

#if WASM
using GLEnum = Silk.NET.OpenGLES.GLEnum;
using static CrossEngine.Platform.Wasm.EGLContext;
#else
using GLEnum = Silk.NET.OpenGL.GLEnum;
using static CrossEngine.Platform.OpenGL.GLContext;
#endif

namespace CrossEngine.Platform.OpenGL
{
    static class GLUtils
    {
        public static GLEnum ToGLBufferUsage(BufferUsageHint bufferUsage)
        {
            switch (bufferUsage)
            {
                case BufferUsageHint.StaticDraw: return GLEnum.StaticDraw;
                case BufferUsageHint.DynamicDraw: return GLEnum.DynamicDraw;
                case BufferUsageHint.StreamDraw: return GLEnum.StreamDraw;
            }

            Debug.Assert(false, $"Unknown {nameof(BufferUsageHint)} value");
            return 0;
        }

        public static GLEnum ToGLIndexDataType(IndexDataType indexType)
        {
            switch (indexType)
            {
                case IndexDataType.UInt: return GLEnum.UnsignedInt;
                case IndexDataType.UShort: return GLEnum.UnsignedShort;
                case IndexDataType.UByte: return GLEnum.UnsignedByte;
            }

            Debug.Assert(false, $"Unknown {nameof(IndexDataType)} value");
            return 0;
        }

        public static GLEnum GetGLBaseDataType(ShaderDataType dataType)
        {
            switch (dataType)
            {
                case ShaderDataType.Float: return GLEnum.Float;
                case ShaderDataType.Float2: return GLEnum.Float;
                case ShaderDataType.Float3: return GLEnum.Float;
                case ShaderDataType.Float4: return GLEnum.Float;

                case ShaderDataType.Mat3: return GLEnum.Float;
                case ShaderDataType.Mat4: return GLEnum.Float;

                case ShaderDataType.Int: return GLEnum.Int;
                case ShaderDataType.Int2: return GLEnum.Int;
                case ShaderDataType.Int3: return GLEnum.Int;
                case ShaderDataType.Int4: return GLEnum.Int;

                case ShaderDataType.Bool: return GLEnum.Bool;
            }

            Debug.Assert(false, $"Unknown {nameof(ShaderDataType)} value");
            return 0;
        }

        public static GLEnum ToGLDrawMode(DrawMode mode)
        {
            switch (mode)
            {
                case DrawMode.Lines: return GLEnum.Lines;
                case DrawMode.Traingles: return GLEnum.Triangles;
                case DrawMode.Points: return GLEnum.Points;
            }

            Debug.Assert(false, $"Unknown {nameof(DrawMode)} value");
            return 0;
        }

        public static GLEnum ToGLBlendFunc(BlendFunc func)
        {
            switch (func)
            {
                case BlendFunc.OneMinusSrcAlpha: return GLEnum.OneMinusSrcAlpha;
                case BlendFunc.One: return GLEnum.One;
            }

            Debug.Assert(false, $"Unknown {nameof(BlendFunc)} value");
            return 0;
        }

        public static GLEnum ToGLDepthFunc(DepthFunc func)
        {
            switch (func)
            {
                case DepthFunc.Never: return GLEnum.Never;
                case DepthFunc.Less: return GLEnum.Less;
                case DepthFunc.Equal: return GLEnum.Equal;
                case DepthFunc.LessEqual: return GLEnum.Lequal;
                case DepthFunc.Greater: return GLEnum.Greater;
                case DepthFunc.NotEqual: return GLEnum.Notequal;
                case DepthFunc.GreaterEqual: return GLEnum.Gequal;
                case DepthFunc.Always: return GLEnum.Always;
            }

            Debug.Assert(false, $"Unknown {nameof(DepthFunc)} value");
            return 0;
        }
        
        public static GLEnum ToGLCullFace(CullFace face)
        {
            switch (face)
            {
                case CullFace.Front: return GLEnum.Front;
                case CullFace.Back: return GLEnum.Back;
            }

            Debug.Assert(false, $"Unknown {nameof(DepthFunc)} value");
            return 0;
        }

        public static GLEnum ToGLFilterParameter(FilterParameter filter)
        {
            switch (filter)
            {
                case FilterParameter.Linear: return GLEnum.Linear;
                case FilterParameter.Nearest: return GLEnum.Nearest;
            }

            Debug.Assert(false, $"Unknown {nameof(FilterParameter)} value");
            return 0;
        }

        internal static GLEnum ToGLWrapParameter(WrapParameter param)
        {
            switch (param)
            {
                case WrapParameter.Repeat: return GLEnum.Repeat;
                case WrapParameter.MirroredRepeat: return GLEnum.MirroredRepeat;
                case WrapParameter.ClampToEdge: return GLEnum.ClampToEdge;
                case WrapParameter.ClampToBorder: return GLEnum.ClampToBorder;
            }

            Debug.Assert(false, $"Unknown {nameof(WrapParameter)} value");
            return 0;
        }

        public static GLEnum ToGLTextureTarget(CrossEngine.Rendering.Textures.TextureTarget target)
        {
            switch (target)
            {
#if !OPENGL_ES
                case CrossEngine.Rendering.Textures.TextureTarget.Texture1D: return GLEnum.Texture1D;
#endif
                case CrossEngine.Rendering.Textures.TextureTarget.Texture2D: return GLEnum.Texture2D;
                case CrossEngine.Rendering.Textures.TextureTarget.Texture3D: return GLEnum.Texture3D;
                case CrossEngine.Rendering.Textures.TextureTarget.TextureCubeMap: return GLEnum.TextureCubeMap;
            }

            Debug.Assert(false, $"Unknown {nameof(CrossEngine.Rendering.Textures.TextureTarget)} value");
            return 0;
        }

        public static GLEnum ToGLPolygonMode(CrossEngine.Rendering.PolygonMode mode)
        {
            switch (mode)
            {
#if !OPENGL_ES
                case CrossEngine.Rendering.PolygonMode.Fill: return GLEnum.Fill;
                case CrossEngine.Rendering.PolygonMode.Line: return GLEnum.Line;
                case CrossEngine.Rendering.PolygonMode.Point: return GLEnum.Point;
#endif
            }

            Debug.Assert(false, $"Unknown {nameof(CrossEngine.Rendering.PolygonMode)} value");
            return 0;
        }

        public static GLEnum ToGLShaderType(CrossEngine.Rendering.Shaders.ShaderType type)
        {
            switch (type)
            {
                case CrossEngine.Rendering.Shaders.ShaderType.Vertex: return GLEnum.VertexShader;
                case CrossEngine.Rendering.Shaders.ShaderType.Geometry: return GLEnum.GeometryShader;
                case CrossEngine.Rendering.Shaders.ShaderType.Fragment: return GLEnum.FragmentShader;
            }

            Debug.Assert(false, $"Unknown {nameof(CrossEngine.Rendering.Shaders.ShaderType)} value");
            return 0;
        }

        public static ShaderDataType ToShaderDataType(GLEnum type)
        {
            switch (type)
            {
                case GLEnum.Float: return ShaderDataType.Float;
                case GLEnum.FloatVec2: return ShaderDataType.Float2;
                case GLEnum.FloatVec3: return ShaderDataType.Float3;
                case GLEnum.FloatVec4: return ShaderDataType.Float4;

                case GLEnum.FloatMat3: return ShaderDataType.Mat3;
                case GLEnum.FloatMat4: return ShaderDataType.Mat4;

                case GLEnum.Int: return ShaderDataType.Int;
                case GLEnum.IntVec2: return ShaderDataType.Int2;
                case GLEnum.IntVec3: return ShaderDataType.Int3;
                case GLEnum.IntVec4: return ShaderDataType.Int4;

                case GLEnum.Bool: return ShaderDataType.Bool;

                case GLEnum.Sampler2D: return ShaderDataType.Sampler2D;
                case GLEnum.SamplerCube: return ShaderDataType.SamplerCube;
            }

            Debug.Assert(false, $"Invalid type or unknown {nameof(ShaderDataType)} value");
            return 0;
        }

        public static GLEnum ToGLColorFormat(ColorFormat format)
        {
            switch (format)
            {
                //case ColorFormat.SingleR: return GL_RED;
                //case ColorFormat.SingleG: throw new NotImplementedException();
                //case ColorFormat.SingleB: throw new NotImplementedException();
                //case ColorFormat.SingleA: throw new NotImplementedException();
                //case ColorFormat.DoubleRG: return GL_RG;
                case ColorFormat.RGB: return GLEnum.Rgb;
                case ColorFormat.RGBA: return GLEnum.Rgba;
#if !OPENGL_ES
                case ColorFormat.BGR: return GLEnum.Bgr;
                case ColorFormat.BGRA: return GLEnum.Bgra;
#endif
            }

            Debug.Assert(false, $"Unknown {nameof(ColorFormat)} value");
            return 0;
        }

        public static GLEnum ToGLTextureFormat(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.ColorRGBA8: return GLEnum.Rgb8;
                case TextureFormat.ColorR32I: return GLEnum.R32i;
                case TextureFormat.ColorRGBA16F: return GLEnum.Rgba16f;
                case TextureFormat.ColorRGBA32F: return GLEnum.Rgba32f;
                case TextureFormat.Depth24Stencil8: return GLEnum.Depth24Stencil8;
            }

            Debug.Assert(false, $"Unknown {nameof(TextureFormat)} value");
            return 0;
        }
    }
}
