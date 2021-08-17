using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using CrossEngine.Rendering.Textures;
using CrossEngine.Logging;
using CrossEngine.Assets.GC;

namespace CrossEngine.Rendering.Buffers
{
    public enum FramebufferTextureFormat : int
    {
        // check utils when modifing this!!

        None = 0,
        // defaults
        DefaultDepth = Depth24Stencil8,
    
        // color
        ColorRGBA8 = GL_RGB8,
        ColorRedInteger = GL_RED_INTEGER,
    
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
        public FramebufferTextureFormat TextureFormat;
        // TODO: filtering/wrap
    
		public FramebufferTextureSpecification(FramebufferTextureFormat format)
        {
            TextureFormat = format;
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
        
        public bool SwapChainTarget;
    }

    public class Framebuffer : IDisposable
    {
        public static uint MaxFramebufferSize = 8192;

        private static class Utils
        {
            public static bool IsDepthFormat(FramebufferTextureFormat format)
            {
                switch (format)
                {
                    case FramebufferTextureFormat.Depth24Stencil8: return true;
                        //case FramebufferTextureFormat.Depth32FStencil8: return true;
                        //case FramebufferTextureFormat.DepthComponent16: return true;
                        //case FramebufferTextureFormat.DepthComponent24: return true;
                        //case FramebufferTextureFormat.DepthComponent32: return true;
                }
                return false;
            }
            public static unsafe void AttachColorTexture(uint id, int internalFormat, int format, int width, int height, int index)
            {
                glTexImage2D(GL_TEXTURE_2D, 0, internalFormat, width, height, 0, format, GL_UNSIGNED_BYTE, null);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
                //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
                //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
                //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

                glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0 + index, GL_TEXTURE_2D, id, 0);
            }
            public static unsafe void AttachDepthTexture(uint id, int internalFormat, int format, int attachmentType, int type, int width, int height)
            {
                glTexImage2D(GL_TEXTURE_2D, 0, internalFormat, width, height, 0, format, type, null);

                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
                //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
                //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
                //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

                glFramebufferTexture2D(GL_FRAMEBUFFER, attachmentType, GL_TEXTURE_2D, id, 0);
            }
        }

        uint fboid = 0;
        //uint rboid = 0;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture Texture { get; private set; }

        //---

        FramebufferSpecification specification;
        List<FramebufferTextureSpecification> colorAttachmentSpecifications = new List<FramebufferTextureSpecification>();
        FramebufferTextureSpecification depthAttachmentSpecification = new FramebufferTextureSpecification(FramebufferTextureFormat.None);
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
                if (!Utils.IsDepthFormat(att.TextureFormat))
                    colorAttachmentSpecifications.Add(att);
                else
                    depthAttachmentSpecification = att;
            }

            Invalidate();

            Log.Core.Trace("created framebuffer (id: {0})", fboid);
        }
        
        unsafe void Invalidate()
        {
            //fixed (uint* p = &fboid)
            //    glGenFramebuffers(1, p);
            //glBindFramebuffer(GL_FRAMEBUFFER, fboid);
            //
            //fixed (uint* p = &colorAttachment)
            //    glGenTextures(1, p);
            //glBindTexture(GL_TEXTURE_2D, colorAttachment);
            //
            //glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, (int)specification.Width, (int)specification.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, null);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            //
            //glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, colorAttachment, 0);
            //
            //fixed (uint* p = &depthAttachment)
            //    glGenTextures(1, p);
            //glBindTexture(GL_TEXTURE_2D, depthAttachment);
            //
            //glTexImage2D(GL_TEXTURE_2D, 0, GL_DEPTH24_STENCIL8, (int)specification.Width, (int)specification.Height, 0, GL_DEPTH_STENCIL, GL_UNSIGNED_INT_24_8, null);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            //
            //glFramebufferTexture2D(GL_FRAMEBUFFER, GL_DEPTH_STENCIL_ATTACHMENT, GL_TEXTURE_2D, depthAttachment, 0);
            //
            //if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            //{
            //    Log.Core.Error("framebuffer is incomplete!");
            //}
            //
            //glBindFramebuffer(GL_FRAMEBUFFER, 0);
            
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

                for (int i = 0; i < _colorAttachments.Count; i++)
                {
                    glBindTexture(GL_TEXTURE_2D, _colorAttachments[i]);
                    switch (colorAttachmentSpecifications[i].TextureFormat)
                    {
                        case FramebufferTextureFormat.ColorRGBA8:
                            Utils.AttachColorTexture(_colorAttachments[i], GL_RGBA8, GL_RGBA, (int)specification.Width, (int)specification.Height, i);
                            break;
                        case FramebufferTextureFormat.ColorRedInteger:
                            Utils.AttachColorTexture(_colorAttachments[i], GL_R32I, GL_RED_INTEGER, (int)specification.Width, (int)specification.Height, i);
                            break;
                        default: Log.Core.Warn("color attachment type not implemented!"); break;
                    }
                }
            }

            // handle depth
            if (depthAttachmentSpecification.TextureFormat != FramebufferTextureFormat.None)
            {
                fixed (uint* p = &_depthAttachment)
                    glGenTextures(1, p);
                glBindTexture(GL_TEXTURE_2D, _depthAttachment);
                switch (depthAttachmentSpecification.TextureFormat)
                {
                    case FramebufferTextureFormat.Depth24Stencil8:
                        Utils.AttachDepthTexture(_depthAttachment, GL_DEPTH24_STENCIL8, GL_DEPTH_STENCIL, GL_DEPTH_STENCIL_ATTACHMENT, GL_UNSIGNED_INT_24_8, (int)specification.Width, (int)specification.Height);
                        break;
                    default: Log.Core.Warn("depth attachment type not implemented!"); break;
                }
            }

            if (_colorAttachments.Count > 1)
            {
                if (_colorAttachments.Count > 4)
                    Log.Core.Error("wtf this is still not implemented");
                int[] buffers = { GL_COLOR_ATTACHMENT0, GL_COLOR_ATTACHMENT1, GL_COLOR_ATTACHMENT2, GL_COLOR_ATTACHMENT3 };
                fixed (int* p = &buffers[0])
                    glDrawBuffers(_colorAttachments.Count, p);
            }
            else if (_colorAttachments.Count == 0)
            {
                // Only depth-pass
                glDrawBuffer(GL_NONE);
            }

            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                Log.Core.Error("framebuffer is incomplete!");
            }

            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            //Log.Core.Trace("invalidated framebuffer (id: {0})", fboid);
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

        public unsafe int ReadPixel(uint attachmentIndex, int x, int y)
        {
            if (attachmentIndex >= _colorAttachments.Count)
            {
                Log.Core.Error("attempted to read framebuffer poixel at {0}, {1}", x, y);
                return 0;
            }

            glReadBuffer(GL_COLOR_ATTACHMENT0 + (int)attachmentIndex);
            int pixelData;
            glReadPixels(x, y, 1, 1, GL_RED_INTEGER, GL_INT, &pixelData);
            return pixelData;
        }

        //void ClearAttachment(uint attachmentIndex, int value)
        //{
        //    if (attachmentIndex >= colorAttachments.Count)
        //    {
        //        Log.Core.Error("index out of range of color attachments");
        //        return;
        //    }
        //
        //    var spec = colorAttachmentSpecifications[(int)attachmentIndex];
        //    
        //    glClearTexImage(colorAttachments[(int)attachmentIndex], 0, (int)spec.TextureFormat, GL_INT, &value);
        //}

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
            Texture.Dispose();

            Log.Core.Warn("unhandled framebuffer disposure (id: {0})", fboid);
            //System.Diagnostics.Debug.Assert(false);
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
            if (Texture != null && Texture.ID != 0)
                Texture.Dispose();
            
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
