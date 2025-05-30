﻿using System;

using System.Diagnostics;

using CrossEngine.Utils;

using CrossEngine.Platform.OpenGL;
#if WINDOWS
using CrossEngine.Platform.Windows;
#endif

namespace CrossEngine.Rendering.Textures
{
    public abstract class Texture : IDisposable
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
        public abstract uint Width { get; }
        public abstract uint Height { get; }
        public IntVec2 Size => new IntVec2((int)Width, (int)Height);

        public abstract void Bind(uint slot = 0);
        public abstract void Unbind();

        public abstract unsafe void SetData(void* data, uint size);

        public abstract void SetFilterParameter(FilterParameter filter);
        public abstract void SetWrapParameter(WrapParameter wrap);

        public static WeakReference<Texture> Create(uint width, uint height, ColorFormat internalFormat)
        {
            return Create(new WeakReference<Texture>(null), width, height, internalFormat);
        }

        // pain
        public static WeakReference<Texture> Create(WeakReference<Texture> wr, uint width, uint height, ColorFormat internalFormat)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: wr.SetTarget(new GLTexture(width, height, internalFormat)); return wr;
#if WINDOWS
                case GraphicsApi.GDI: wr.SetTarget(new GdiTexture(width, height, internalFormat)); return wr;
#endif
            }

            Debug.Assert(false, $"Udefined {nameof(GraphicsApi)} value");
            return null;
        }
    }

    public enum FilterParameter
    {
        None = 0,

        Linear,
        Nearest,

        Default = Linear,
    }

    public enum WrapParameter
    {
        None = 0,

        Repeat,
        MirroredRepeat,
        ClampToEdge, // tex coords clamped
        ClampToBorder, // tex coords outside have user specified color

        Default = Repeat,
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

    public enum ColorFormat
    {
        None = 0,

        //SingleR,
        //SingleG,
        //SingleB,
        //SingleA,
        //
        //DoubleRG,

        RGB,
        BGR,

        RGBA,
        BGRA,

        Luminance,

        // GL_COLOR_INDEX
        // GL_LUMINANCE_ALPHA
    }
}
