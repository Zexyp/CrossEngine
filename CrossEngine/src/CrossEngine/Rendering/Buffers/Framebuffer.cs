﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Rendering.Textures;
using CrossEngine.Utils;
using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering.Buffers
{
    public enum TextureFormat
    {
        // !check utils when modifing this!!
        None = 0,

        // color
        ColorRGBA8,
        ColorR32I,
        ColorRGBA32F,

        // depth and stencil
        Depth24Stencil8,
        //Depth32FStencil8 = GL_DEPTH32F_STENCIL8,

        // only depth
        //DepthComponent32 = GL_DEPTH_COMPONENT32,
        //DepthComponent24 = GL_DEPTH_COMPONENT24,
        //DepthComponent16 = GL_DEPTH_COMPONENT16,

        // defaults
        DefaultDepth = Depth24Stencil8,
    }

    public struct FramebufferTextureSpecification
    {
        public TextureFormat Format;
        public FilterParameter Filter;
        public WrapParameter Wrap;
        // TODO: wrap

        internal bool dontDraw;

        public FramebufferTextureSpecification(TextureFormat format, FilterParameter filter = FilterParameter.Default, WrapParameter wrap = WrapParameter.Default)
        {
            Format = format;
            Filter = filter;
            Wrap = wrap;

            dontDraw = false;
        }
    }

    public struct FramebufferAttachmentSpecification
    {
        public FramebufferAttachmentSpecification(params FramebufferTextureSpecification[] attachments)
        {
            Attachments = new List<FramebufferTextureSpecification>(attachments);
        }

        public List<FramebufferTextureSpecification> Attachments;
    }

    public struct FramebufferSpecification
    {
        public uint Width, Height;
        public FramebufferAttachmentSpecification Attachments;
    }

    public abstract class Framebuffer : IDisposable
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

        ~Framebuffer()
        {
            Dispose(false);
        }

        public abstract void Bind();
        public abstract void Unbind();

        public abstract void Resize(uint width, uint height);
        public abstract void ClearAttachment(int attachmentIndex, int value);
        public abstract int ReadPixel(int attachmentIndex, uint x, uint y);
        public abstract uint GetColorAttachmentRendererID(int index = 0);
        //public abstract ref FramebufferSpecification GetSpecification();

        public static unsafe Ref<Framebuffer> Create(ref FramebufferSpecification specification)
        {
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case RendererAPI.API.OpenGL: return (Ref<Framebuffer>)new GLFramebuffer(ref specification);
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }
    }
}
