﻿using System;

using System.Diagnostics;

using CrossEngine.Utils;
using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering.Textures
{
    public abstract class Texture
    {
        public bool Disposed { get; protected set; } = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                // free any other managed objects here
            }

            // free any unmanaged objects here

            Disposed = true;
        }

        ~Texture()
        {
            Dispose(false);
        }

        public TextureTarget Target { get; protected set; }

        public abstract uint RendererId { get; }

        public abstract void Bind(uint slot = 0);
        public abstract void Unbind();

        public abstract unsafe void SetData(void* data, uint size);

        public abstract void SetFilterParameter(FilterParameter filter);
        public abstract void SetWrapParameter(WrapParameter wrap);

        public static Ref<Texture> Create(uint width, uint height, ColorChannelFormat internalFormat)
        {
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case RendererAPI.API.OpenGL: return (Ref<Texture>)new GLTexture(width, height, internalFormat);
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }

    public enum FilterParameter
    {
        None = 0,

        Linear,
        Nearest,
    }

    public enum WrapParameter
    {
        None = 0,

        Repeat,
        MirroredRepeat,
        ClampToEdge, // tex coords clamped
        ClampToBorder, // tex coords outside have user specified color
    }

    public enum TextureTarget
    {
        None = 0,

        Texture1D,
        Texture2D,
        Texture3D,

        TextureCubeMap,

        //TextureCubeMapPositiveX = GL_TEXTURE_CUBE_MAP_POSITIVE_X,
        //TextureCubeMapNegativeX = GL_TEXTURE_CUBE_MAP_NEGATIVE_X,
        //TextureCubeMapPositiveY = GL_TEXTURE_CUBE_MAP_POSITIVE_Y,
        //TextureCubeMapNegativeY = GL_TEXTURE_CUBE_MAP_NEGATIVE_Y,
        //TextureCubeMapPositiveZ = GL_TEXTURE_CUBE_MAP_POSITIVE_Z,
        //TextureCubeMapNegativeZ = GL_TEXTURE_CUBE_MAP_NEGATIVE_Z,
    }

    public enum ColorChannelFormat
    {
        None = 0,

        SingleR,
        SingleG,
        SingleB,
        SingleA,

        DoubleRG,

        TripleRGB,
        TripleBGR,

        QuadrupleRGBA,
        QuadrupleBGRA,

        // GL_COLOR_INDEX
        // GL_LUMINANCE
        // GL_LUMINANCE_ALPHA
    }
}
