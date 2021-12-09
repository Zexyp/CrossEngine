using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using CrossEngine.Rendering.Textures;
using CrossEngine.Logging;
using CrossEngine.Assets.GC;
using CrossEngine.Profiling;

namespace CrossEngine.Rendering.Buffers
{
    public enum TextureFormat : int
    {
        // !check utils when modifing this!!
        None = GL_NONE,

        // defaults
        DefaultDepth = Depth24Stencil8,
    
        // color
        ColorRGBA8 = GL_RGB8,
        ColorR32I = GL_R32I,
        ColorRGBA32F = GL_RGBA32F,
    
        // depth and stencil
        Depth24Stencil8 = GL_DEPTH24_STENCIL8,
        //Depth32FStencil8 = GL_DEPTH32F_STENCIL8,
    
        // only depth
        //DepthComponent32 = GL_DEPTH_COMPONENT32,
        //DepthComponent24 = GL_DEPTH_COMPONENT24,
        //DepthComponent16 = GL_DEPTH_COMPONENT16,
    }
    
    public struct FramebufferTextureSpecification
    {
        public TextureFormat Format;
        public FilterParameter Filter;
        // TODO: wrap

        public int? Index;

        internal bool dontDraw;

		public FramebufferTextureSpecification(TextureFormat format, FilterParameter filter = FilterParameter.Linear)
        {
            Format = format;
            Filter = filter;

            Index = null;

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

    public class Framebuffer : IDisposable
    {
        public static uint MaxFramebufferSize = 8192;

        private static class Utils
        {
            public static bool IsDepthFormat(TextureFormat format)
            {
                switch (format)
                {
                    case TextureFormat.Depth24Stencil8: return true;
                    //case FramebufferTextureFormat.Depth32FStencil8: return true;
                    //case FramebufferTextureFormat.DepthComponent16: return true;
                    //case FramebufferTextureFormat.DepthComponent24: return true;
                    //case FramebufferTextureFormat.DepthComponent32: return true;
                }
                return false;
            }

            public static int GetColorFormat(TextureFormat format)
            {
                switch (format)
                {
                    case TextureFormat.ColorRGBA8: return GL_RGBA;
                    case TextureFormat.ColorRGBA32F: return GL_RGBA;
                    case TextureFormat.ColorR32I: return GL_RED_INTEGER;
                }
                Log.Core.Error("unknown attachment type!");
                throw new NotImplementedException();
            }
        }

        uint fboid = 0;
        //uint rboid = 0;

        public int Width { get; private set; }
        public int Height { get; private set; }

        //---

        FramebufferSpecification specification;
        List<FramebufferTextureSpecification> colorAttachmentSpecifications = new List<FramebufferTextureSpecification>();
        FramebufferTextureSpecification depthAttachmentSpecification = new FramebufferTextureSpecification(TextureFormat.None);
        List<uint> _colorAttachments = new List<uint>();
        public ReadOnlyCollection<uint> ColorAttachments { get => _colorAttachments.AsReadOnly(); }
        uint _depthAttachment;
        public uint DepthAttachment { get => _depthAttachment; }
        //uint colorAttachment;

        public unsafe Framebuffer(FramebufferSpecification spec)
        {
            specification = spec;
            foreach (FramebufferTextureSpecification att in spec.Attachments.Attachments)
            {
                if (!Utils.IsDepthFormat(att.Format))
                    colorAttachmentSpecifications.Add(att);
                else
                    depthAttachmentSpecification = att;
            }

            Invalidate();

            Log.Core.Trace("created framebuffer (id: {0})", fboid);
        }
        
        unsafe void Invalidate()
        {
            Profiler.BeginScope($"{nameof(Framebuffer)}.{nameof(Framebuffer.Invalidate)}");

            if (fboid != 0)
            {
                fixed (uint* p = &fboid)
                    glDeleteFramebuffers(1, p);
                fixed (uint* p = &_colorAttachments.ToArray()[0])
                    glDeleteTextures(_colorAttachments.Count, p);
                fixed (uint* p = &_depthAttachment)
                    glDeleteTextures(1, p);
        
                _colorAttachments.Clear();
                _depthAttachment = 0;
            }
            
            fixed (uint* p = &fboid)
                glGenFramebuffers(1, p);
            glBindFramebuffer(GL_FRAMEBUFFER, fboid);
            
            // handle color
            if (colorAttachmentSpecifications.Count > 0)
            {
                _colorAttachments.Capacity = colorAttachmentSpecifications.Count;
                uint[] arr = new uint[colorAttachmentSpecifications.Count];
                fixed (uint* p = &arr[0])
                    glGenTextures(arr.Length, p);
                _colorAttachments.Clear();
                _colorAttachments.AddRange(arr);

                // index begins at 0
                for (int i = 0; i < _colorAttachments.Count; i++)
                {
                    glBindTexture(GL_TEXTURE_2D, _colorAttachments[i]);
                    AttachColorTexture(
                        _colorAttachments[i],
                        colorAttachmentSpecifications[i],
                        (int)specification.Width,
                        (int)specification.Height,
                        colorAttachmentSpecifications[i].Index.HasValue ? colorAttachmentSpecifications[i].Index.Value : i);
                }
            }

            // handle depth
            if (depthAttachmentSpecification.Format != TextureFormat.None)
            {
                fixed (uint* p = &_depthAttachment)
                    glGenTextures(1, p);
                glBindTexture(GL_TEXTURE_2D, _depthAttachment);
                switch (depthAttachmentSpecification.Format)
                {
                    case TextureFormat.Depth24Stencil8:
                        AttachDepthTexture(
                            _depthAttachment,
                            GL_DEPTH24_STENCIL8,
                            GL_DEPTH_STENCIL,
                            GL_DEPTH_STENCIL_ATTACHMENT,
                            GL_UNSIGNED_INT_24_8,
                            (int)specification.Width,
                            (int)specification.Height,
                            (int)depthAttachmentSpecification.Filter);
                        break;
                    default: Log.Core.Warn("depth attachment type not implemented!"); break;
                }
            }

            SetDrawBuffers();

            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                Log.Core.Error("framebuffer is incomplete!");
            }

            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            //Log.Core.Trace("invalidated framebuffer (id: {0})", fboid);
        }

        private unsafe void SetDrawBuffers()
        {
            if (_colorAttachments.Count > 1)
            {
                int[] buffers = new int[_colorAttachments.Count];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = (colorAttachmentSpecifications[i].dontDraw) ? GL_NONE : GL_COLOR_ATTACHMENT0 + i;
                }
                fixed (int* p = &buffers[0])
                    glDrawBuffers(buffers.Length, p);
            }
            else if (_colorAttachments.Count == 0)
            {
                // Only depth-pass
                glDrawBuffer(GL_NONE);
            }

            Profiler.EndScope();
        }

        private static unsafe void AttachColorTexture(uint id, FramebufferTextureSpecification spec, int width, int height, int index)
        {
            glTexImage2D(GL_TEXTURE_2D, 0, (int)spec.Format, width, height, 0, Utils.GetColorFormat(spec.Format), GL_UNSIGNED_BYTE, null);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)spec.Filter);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)spec.Filter);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0 + index, GL_TEXTURE_2D, id, 0);
        }

        private static unsafe void AttachDepthTexture(uint id, int internalFormat, int format, int attachmentType, int type, int width, int height, int filter = GL_LINEAR)
        {
            glTexImage2D(GL_TEXTURE_2D, 0, internalFormat, width, height, 0, format, type, null);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filter);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filter);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

            glFramebufferTexture2D(GL_FRAMEBUFFER, attachmentType, GL_TEXTURE_2D, id, 0);
        }

        public void Bind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, fboid);
            glViewport(0, 0, (int)specification.Width, (int)specification.Height);
        }

        public void Resize(uint width, uint height)
        {
            if (width == 0 || height == 0 || width > MaxFramebufferSize || height > MaxFramebufferSize)
            {
                Log.Core.Warn("attempted to rezize framebuffer to {0}, {1}", width, height);
                return;
            }
            specification.Width = width;
            specification.Height = height;

            Invalidate();
        }

        // don't forget it's flipped y ...
        public unsafe int ReadPixel(uint attachmentIndex, int x, int y)
        {
            if (attachmentIndex >= _colorAttachments.Count)
            {
                Log.Core.Error("attempted to read framebuffer poixel at {0}, {1}", x, y);
                return 0;
            }

            glReadBuffer(GL_COLOR_ATTACHMENT0 + (int)attachmentIndex);
            int pixelData;

            glReadPixels(x, y, 1, 1, Utils.GetColorFormat(colorAttachmentSpecifications[(int)attachmentIndex].Format), GL_INT, &pixelData);
            return pixelData;
        }

        public unsafe void ClearAttachment(uint attachmentIndex, int value)
        {
            if (attachmentIndex >= _colorAttachments.Count)
            {
                Log.Core.Error("index out of range of color attachments");
                return;
            }
        
            var spec = colorAttachmentSpecifications[(int)attachmentIndex];

            //(int)_colorAttachments[(int)attachmentIndex]
            glClearBufferiv(GL_COLOR, (int)fboid, &value);
        }

        public void EnableColorDrawBuffer(int index, bool enable)
        {
            if (index >= colorAttachmentSpecifications.Count)
            {
                Log.Core.Error("index out of range of color attachments");
                return;
            }

            FramebufferTextureSpecification s = colorAttachmentSpecifications[index];
            s.dontDraw = !enable;
            colorAttachmentSpecifications[index] = s;

            SetDrawBuffers();
        }

        public void EnableAllColorDrawBuffers(bool enable)
        {
            for (int i = 0; i < colorAttachmentSpecifications.Count; i++)
            {
                FramebufferTextureSpecification s = colorAttachmentSpecifications[i];
                s.dontDraw = !enable;
                colorAttachmentSpecifications[i] = s;
            }

            SetDrawBuffers();
        }

        //---

        //public unsafe Framebuffer(int width, int height)
        //{
        //    Texture = new Texture(width, height, ColorChannelFormat.TripleRGB);
        //    Texture.Bind();
        //
        //    // create framebuffer
        //    fixed(uint* fbop = &fboid)
        //        glGenFramebuffers(1, fbop);
        //    // bind it
        //    glBindFramebuffer(GL_FRAMEBUFFER, fboid);
        //
        //    // set color attachment
        //    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, Texture.ID, 0);
        //
        //    // create a renderbuffer object for depth and stencil attachment (we won't be sampling these (for now... ( ͡° ͜ʖ ͡°)))
        //    fixed (uint* rbop = &rboid)
        //        glGenRenderbuffers(1, rbop);
        //    glBindRenderbuffer(rboid);
        //    glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, width, height); //GL_DEPTH_COMPONENT32, GL_DEPTH_COMPONENT24, GL_DEPTH_COMPONENT16 but we can also add stencil GL_DEPTH24_STENCIL8, GL_DEPTH32F_STENCIL8
        //    glFramebufferRenderbuffer(GL_FRAMEBUFFER, GL_DEPTH_ATTACHMENT, GL_RENDERBUFFER, rboid); //or GL_DEPTH_STENCIL_ATTACHMENT when having stencil
        //
        //    // just check if it's happy
        //    if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
        //    {
        //        Log.Core.Error("framebuffer is not complete!");
        //    }
        //
        //    // just unbind to display the frame properly
        //    glBindFramebuffer(GL_FRAMEBUFFER, 0);
        //
        //    this.Width = width;
        //    this.Height = height;
        //
        //    Log.Core.Trace("created framebuffer (id: {0})", fboid);
        //}

        #region IDisposable
        // cleanup
        ~Framebuffer()
        {
            Log.Core.Warn("unhandled framebuffer disposure (id: {0})", fboid);
            //System.Diagnostics.Debug.Assert(false);
            for (int i = 0; i < _colorAttachments.Count; i++)
                GPUGarbageCollector.MarkObject(GPUObjectType.Texture, _colorAttachments[i]);
            if (_depthAttachment != 0) GPUGarbageCollector.MarkObject(GPUObjectType.Texture, _depthAttachment);

            GPUGarbageCollector.MarkObject(GPUObjectType.Framebuffer, fboid);
            //if (rboid != 0)
            //    GPUGarbageCollector.MarkObject(GPUObjectType.Renderbuffer, rboid);
            return;

            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (fboid != 0)
            {
                Log.Core.Trace("deleting framebuffer (id: {0})", fboid);

                fixed (uint* fbop = &fboid)
                    glDeleteFramebuffers(1, fbop);
                fboid = 0;

                //if (rboid != 0)
                //{
                //    fixed (uint* rbop = &rboid)
                //        glDeleteRenderbuffers(1, rbop);
                //    rboid = 0;
                //}
            }
        }
        #endregion

        //public unsafe void Resize(int width, int height)
        //{
        //    glBindRenderbuffer(rbo);
        //    glRenderbufferStorage(GL_RENDERBUFFER, GL_DEPTH_COMPONENT24, width, height);
        //
        //    Texture.SetData(null, width, height, ColorChannelFormat.TripleRGB, ColorChannelFormat.TripleRGB);
        //
        //    Width = width;
        //    Height = height;
        //}

        public static void Unbind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, 0);
        }
    }
}
