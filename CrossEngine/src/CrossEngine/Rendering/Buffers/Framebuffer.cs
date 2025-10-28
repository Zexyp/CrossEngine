using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;
using CrossEngine.Rendering.Textures;
using CrossEngine.Utils;

using CrossEngine.Platform.OpenGL;
using CrossEngine.Utils.Structs;

namespace CrossEngine.Rendering.Buffers
{
    public enum TextureFormat
    {
        // !check utils when modifing this!!
        None = 0,

        // color
        ColorRGBA8,
        ColorR32I,
        ColorRGB16F,
        ColorRGBA16F,
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
        // TODO: enabled

        public bool Enabled = true;

        public FramebufferTextureSpecification(TextureFormat format, FilterParameter filter = FilterParameter.Default, WrapParameter wrap = WrapParameter.Default)
        {
            Format = format;
            Filter = filter;
            Wrap = wrap;

            Enabled = true;
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

    public abstract class Framebuffer : GpuObject
    {
        public abstract uint Width { get; }
        public abstract uint Height { get; }
        public IntVec2 Size => new IntVec2((int)Width, (int)Height);

        public abstract void Bind();
        public abstract void Unbind();

        public abstract void Resize(uint width, uint height);
        public abstract void ClearAttachment(int attachmentIndex, IntVec4 value);
        public abstract void ClearAttachment(int attachmentIndex, Vector4 value);
        public abstract int ReadPixel(int attachmentIndex, uint x, uint y);
        public abstract uint GetColorAttachmentRendererID(int attachmentIndex = 0);
        public abstract uint GetDepthAttachmentRendererID();
        public abstract void BindColorAttachment(int attachmentIndex = 0, uint slot = 0);
        public abstract void BindDepthAttachment(uint slot = 0);
        public abstract void BlitTo(Framebuffer? target, IList<(int from, int to)> attachmentIndexes = null);
        public abstract void BlitDepthTo(Framebuffer? target);
        public abstract void EnableColorAttachments(IList<int> attachmentIndexes = null);
        //public abstract ref FramebufferSpecification GetSpecification();

        public static unsafe Framebuffer Create(in FramebufferSpecification specification)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new GLFramebuffer(in specification);
            }

            Debug.Assert(false, $"Undefined {nameof(GraphicsApi)} value");
            return null;
        }
    }
}
