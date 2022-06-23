using System;
using static OpenGL.GL;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

using CrossEngine.Rendering.Textures;
using CrossEngine.Logging;
using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;

namespace CrossEngine.Platform.OpenGL
{
    public class GLFramebuffer : Framebuffer
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
                    case TextureFormat.Depth24Stencil8: return GL_DEPTH_STENCIL;
                }

                Debug.Assert(false, $"Unknown {nameof(TextureFormat)} value");
                return 0;
            }

            public static int GetAttachmentType(TextureFormat format)
            {
                switch (format)
                {
                    case TextureFormat.Depth24Stencil8: return GL_DEPTH_STENCIL_ATTACHMENT;
                }

                Debug.Assert(false, $"Unknown {nameof(TextureFormat)} value");
                return 0;
            }
        }

        internal uint _rendererId = 0;
        //uint rboid = 0;

        public uint Width => specification.Width;
        public uint Height => specification.Height;

        //---

        FramebufferSpecification specification;
        List<FramebufferTextureSpecification> colorAttachmentSpecifications = new List<FramebufferTextureSpecification>();
        FramebufferTextureSpecification depthAttachmentSpecification = new FramebufferTextureSpecification(TextureFormat.None);
        readonly List<uint> _colorAttachments = new List<uint>();
        public readonly ReadOnlyCollection<uint> ColorAttachments;
        uint _depthAttachment;
        public uint DepthAttachment { get => _depthAttachment; }
        //uint colorAttachment;

        public unsafe GLFramebuffer(ref FramebufferSpecification spec)
        {
            ColorAttachments = _colorAttachments.AsReadOnly();

            Profiler.Function();

            specification = spec;
            foreach (FramebufferTextureSpecification att in spec.Attachments.Attachments)
            {
                if (!Utils.IsDepthFormat(att.Format))
                    colorAttachmentSpecifications.Add(att);
                else
                    depthAttachmentSpecification = att;
            }

            Invalidate();
        }

        protected override unsafe void Dispose(bool disposing)
        {
            Profiler.Function();

            if (Disposed)
                return;

            if (disposing)
            {
                // free any other managed objects here
            }

            // free any unmanaged objects here
            fixed (uint* p = &_rendererId)
                glDeleteFramebuffers(1, p);
            fixed (uint* p = &_colorAttachments.ToArray()[0])
                glDeleteTextures(_colorAttachments.Count, p);
            fixed (uint* p = &_depthAttachment)
                glDeleteTextures(1, p);

            Disposed = true;
        }

        unsafe private void Invalidate()
        {
            Profiler.BeginScope($"{nameof(GLFramebuffer)}.{nameof(GLFramebuffer.Invalidate)}");

            if (_rendererId != 0)
            {
                fixed (uint* p = &_rendererId)
                    glDeleteFramebuffers(1, p);
                fixed (uint* p = &_colorAttachments.ToArray()[0])
                    glDeleteTextures(_colorAttachments.Count, p);
                fixed (uint* p = &_depthAttachment)
                    glDeleteTextures(1, p);
        
                _colorAttachments.Clear();
                _depthAttachment = 0;
            }
            
            fixed (uint* p = &_rendererId)
                glGenFramebuffers(1, p);
            glBindFramebuffer(GL_FRAMEBUFFER, _rendererId);
            
            // handle color
            if (colorAttachmentSpecifications.Count > 0)
            {
                _colorAttachments.Capacity = colorAttachmentSpecifications.Count;
                uint[] arr = new uint[colorAttachmentSpecifications.Count];
                fixed (uint* p = &arr[0])
                    glGenTextures(arr.Length, p);
                _colorAttachments.AddRange(arr);

                // attachment index begins at 0
                for (int i = 0; i < _colorAttachments.Count; i++)
                {
                    glBindTexture(GL_TEXTURE_2D, _colorAttachments [i]);
                    AttachColorTexture(
                        _colorAttachments[i],
                        colorAttachmentSpecifications[i],
                        specification.Width,
                        specification.Height,
                        i);
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
                            depthAttachmentSpecification,
                            specification.Width,
                            specification.Height);
                        break;
                    default: Application.CoreLog.Warn("depth attachment type not implemented!"); break;
                }
            }

            SetDrawBuffers();

            if (glCheckFramebufferStatus(GL_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE)
            {
                Application.CoreLog.Error("framebuffer is incomplete!");
            }

            glBindFramebuffer(GL_FRAMEBUFFER, 0);

            //Log.Core.Trace("invalidated framebuffer (id: {0})", fboid);
            Profiler.EndScope();
        }

        private static unsafe void AttachColorTexture(uint texid, FramebufferTextureSpecification spec, uint width, uint height, int index)
        {
            glTexImage2D(
                GL_TEXTURE_2D,
                0,
                GLUtils.ToGLTextureFormat(spec.Format),
                (int)width,
                (int)height,
                0,
                Utils.GetColorFormat(spec.Format),
                GL_UNSIGNED_BYTE,
                null);

            int filt = GLUtils.ToGLFilterParameter(spec.Filter);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filt);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filt);
            int wrap = GLUtils.ToGLWrapParameter(spec.Wrap);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, wrap);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, wrap);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, wrap);

            glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0 + index, GL_TEXTURE_2D, texid, 0);
        }

        private static unsafe void AttachDepthTexture(uint texid, FramebufferTextureSpecification spec, uint width, uint height)
        {
            glTexImage2D(
                GL_TEXTURE_2D,
                0,
                GLUtils.ToGLTextureFormat(spec.Format),
                (int)width,
                (int)height,
                0,
                Utils.GetColorFormat(spec.Format),
                GL_UNSIGNED_INT_24_8, // we need to care about the type or opengl will get mad
                null);

            int filt = GLUtils.ToGLFilterParameter(spec.Filter);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filt);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filt);
            int wrap = GLUtils.ToGLWrapParameter(spec.Wrap);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, wrap);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, wrap);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, wrap);

            glFramebufferTexture2D(GL_FRAMEBUFFER, Utils.GetAttachmentType(spec.Format), GL_TEXTURE_2D, texid, 0);
        }
        
        public override void Bind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, _rendererId);
            glViewport(0, 0, (int)specification.Width, (int)specification.Height);
        }

        public override void Unbind()
        {
            glBindFramebuffer(GL_FRAMEBUFFER, 0);
        }

        public override void Resize(uint width, uint height)
        {
            if (width == 0 || height == 0 || width > MaxFramebufferSize || height > MaxFramebufferSize)
            {
                Application.CoreLog.Warn($"attempted to rezize framebuffer to {width}, {height}");
                return;
            }
            specification.Width = width;
            specification.Height = height;

            Invalidate();
        }

        // don't forget it's flipped y ...
        public override unsafe int ReadPixel(int attachmentIndex, uint x, uint y)
        {
            Debug.Assert(attachmentIndex < colorAttachmentSpecifications.Count);

            glBindFramebuffer(_rendererId);
            
            glReadBuffer(GL_COLOR_ATTACHMENT0 + attachmentIndex);
            int pixelData;
            glReadPixels((int)x, (int)y, 1, 1, Utils.GetColorFormat(colorAttachmentSpecifications[attachmentIndex].Format), GL_INT, &pixelData);
            return pixelData;
        }

        public override unsafe void ClearAttachment(int attachmentIndex, int value)
        {
            Debug.Assert(attachmentIndex < colorAttachmentSpecifications.Count);

            //var spec = colorAttachmentSpecifications[attachmentIndex];

            //(int)_colorAttachments[(int)attachmentIndex]
            glBindFramebuffer(_rendererId);

            glClearBufferiv(GL_COLOR, (int)_rendererId, &value);
        }

        public void EnableColorAttachment(int attachmentIndex, bool enable)
        {
            Debug.Assert(attachmentIndex < colorAttachmentSpecifications.Count);

            FramebufferTextureSpecification s = colorAttachmentSpecifications[attachmentIndex];
            s.dontDraw = !enable;
            colorAttachmentSpecifications[attachmentIndex] = s;

            SetDrawBuffers();
        }

        public void EnableAllColorAttachments(bool enable)
        {
            for (int i = 0; i < colorAttachmentSpecifications.Count; i++)
            {
                FramebufferTextureSpecification s = colorAttachmentSpecifications[i];
                s.dontDraw = !enable;
                colorAttachmentSpecifications[i] = s;
            }

            SetDrawBuffers();
        }

        public override uint GetColorAttachmentRendererID(int attachmentIndex = 0)
        {
            Debug.Assert(attachmentIndex < colorAttachmentSpecifications.Count);
            return _colorAttachments[attachmentIndex];
        }

        public void CopyToScreen()
        {
            glBindFramebuffer(GL_READ_FRAMEBUFFER, _rendererId);
            glBindFramebuffer(GL_DRAW_FRAMEBUFFER, 0);
            glBlitFramebuffer(0, 0, (int)specification.Width, (int)specification.Height, 0, 0, (int)specification.Width, (int)specification.Height,
                              GL_COLOR_BUFFER_BIT, GL_NEAREST);
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
    }
}
