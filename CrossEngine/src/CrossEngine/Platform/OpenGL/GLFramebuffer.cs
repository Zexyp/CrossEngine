using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

using CrossEngine.Rendering.Textures;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Profiling;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Debugging;
using CrossEngine.Utils;

#if WASM
using GLEnum = Silk.NET.OpenGLES.GLEnum;
using static CrossEngine.Platform.Wasm.EGLContext;
#else
using GLEnum = Silk.NET.OpenGL.GLEnum;
using static CrossEngine.Platform.OpenGL.GLContext;
#endif

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

            public static GLEnum GetColorFormat(TextureFormat format)
            {
                switch (format)
                {
                    case TextureFormat.ColorRGBA8: return GLEnum.Rgba;
                    case TextureFormat.ColorRGBA16F: return GLEnum.Rgba;
                    case TextureFormat.ColorRGBA32F: return GLEnum.Rgba;
                    case TextureFormat.ColorR32I: return GLEnum.RedInteger;
                    case TextureFormat.Depth24Stencil8: return GLEnum.DepthStencil;
                }

                Debug.Assert(false, $"Unknown {nameof(TextureFormat)} value");
                return 0;
            }

            public static GLEnum GetAttachmentType(TextureFormat format)
            {
                switch (format)
                {
                    case TextureFormat.Depth24Stencil8: return GLEnum.DepthStencilAttachment;
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

        public unsafe GLFramebuffer(in FramebufferSpecification spec)
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

            GC.KeepAlive(this);
            GPUGC.Register(this);

            RendererApi.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
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
                gl.DeleteFramebuffers(1, p);
            fixed (uint* p = &_colorAttachments.ToArray()[0])
                gl.DeleteTextures((uint)_colorAttachments.Count, p);
            fixed (uint* p = &_depthAttachment)
                gl.DeleteTextures(1, p);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererApi.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }
        
        public override void Bind()
        {
            gl.BindFramebuffer(GLEnum.Framebuffer, _rendererId);
            gl.Viewport(0, 0, specification.Width, specification.Height);
        }

        public override void Unbind()
        {
            gl.BindFramebuffer(GLEnum.Framebuffer, 0);
        }

        public override void Resize(uint width, uint height)
        {
            if (width == 0 || height == 0 || width > MaxFramebufferSize || height > MaxFramebufferSize)
            {
                RendererApi.Log.Warn($"attempted to rezize framebuffer to {width}, {height}");
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

            gl.BindFramebuffer(GLEnum.Framebuffer, _rendererId);
            
            gl.ReadBuffer(GLEnum.ColorAttachment0 + attachmentIndex);
            int pixelData;
            gl.ReadPixels((int)x, (int)y, 1, 1, Utils.GetColorFormat(colorAttachmentSpecifications[attachmentIndex].Format), GLEnum.Int, &pixelData);
            return pixelData;
        }

        public override unsafe void ClearAttachment(int attachmentIndex, int value)
        {
            Debug.Assert(attachmentIndex < colorAttachmentSpecifications.Count);

            //var spec = colorAttachmentSpecifications[attachmentIndex];

            //(int)_colorAttachments[(int)attachmentIndex]
            gl.BindFramebuffer(GLEnum.Framebuffer, _rendererId);

            gl.ClearBuffer(GLEnum.Color, (int)_rendererId, &value);
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
            gl.BindFramebuffer(GLEnum.ReadFramebuffer, _rendererId);
            gl.BindFramebuffer(GLEnum.DrawFramebuffer, 0);
            gl.BlitFramebuffer(0, 0, (int)specification.Width, (int)specification.Height, 0, 0, (int)specification.Width, (int)specification.Height,
                              (int)GLEnum.ColorBufferBit, GLEnum.Nearest);
        }

        private unsafe void SetDrawBuffers()
        {
            if (_colorAttachments.Count > 1)
            {
                int[] buffers = new int[_colorAttachments.Count];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = (int)(colorAttachmentSpecifications[i].dontDraw ? GLEnum.None : GLEnum.ColorAttachment0 + i);
                }
                fixed (void* p = & buffers[0])
                    gl.DrawBuffers((uint)buffers.Length, (GLEnum*)p);
            }
            else if (_colorAttachments.Count == 0)
            {
                // Only depth-pass
#if !OPENGL_ES
                gl.DrawBuffer(GLEnum.None);
#else
                gl.DrawBuffers(0, (GLEnum*)null);
#endif
            }
        }
        
        unsafe private void Invalidate()
        {
            Profiler.BeginScope($"{nameof(GLFramebuffer)}.{nameof(GLFramebuffer.Invalidate)}");

            if (_rendererId != 0)
            {
                fixed (uint* p = &_rendererId)
                    gl.DeleteFramebuffers(1, p);
                fixed (uint* p = &_colorAttachments.ToArray()[0])
                    gl.DeleteTextures((uint)_colorAttachments.Count, p);
                fixed (uint* p = &_depthAttachment)
                    gl.DeleteTextures(1, p);
        
                _colorAttachments.Clear();
                _depthAttachment = 0;
            }
            
            fixed (uint* p = &_rendererId)
                gl.GenFramebuffers(1, p);
            gl.BindFramebuffer(GLEnum.Framebuffer, _rendererId);
            
            // handle color
            if (colorAttachmentSpecifications.Count > 0)
            {
                _colorAttachments.Capacity = colorAttachmentSpecifications.Count;
                uint[] arr = new uint[colorAttachmentSpecifications.Count];
                fixed (uint* p = &arr[0])
                    gl.GenTextures((uint)arr.Length, p);
                _colorAttachments.AddRange(arr);

                // attachment index begins at 0
                for (int i = 0; i < _colorAttachments.Count; i++)
                {
                    gl.BindTexture(GLEnum.Texture2D, _colorAttachments [i]);
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
                    gl.GenTextures(1, p);
                gl.BindTexture(GLEnum.Texture2D, _depthAttachment);
                switch (depthAttachmentSpecification.Format)
                {
                    case TextureFormat.Depth24Stencil8:
                        AttachDepthTexture(
                            _depthAttachment,
                            depthAttachmentSpecification,
                            specification.Width,
                            specification.Height);
                        break;
                    default: RendererApi.Log.Warn("depth attachment type not implemented!"); break;
                }
            }

            SetDrawBuffers();

            if (gl.CheckFramebufferStatus(GLEnum.Framebuffer) != GLEnum.FramebufferComplete)
            {
                RendererApi.Log.Error("framebuffer is incomplete!");
            }

            gl.BindFramebuffer(GLEnum.Framebuffer, 0);

            //Log.Core.Trace("invalidated framebuffer (id: {0})", fboid);
            Profiler.EndScope();
        }

        private static unsafe void AttachColorTexture(uint texid, FramebufferTextureSpecification spec, uint width, uint height, int index)
        {
            gl.TexImage2D(
                GLEnum.Texture2D,
                0,
                (int)GLUtils.ToGLTextureFormat(spec.Format),
                width,
                height,
                0,
                Utils.GetColorFormat(spec.Format),
                GLEnum.UnsignedByte,
                null);

            int filt = (int)GLUtils.ToGLFilterParameter(spec.Filter);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, filt);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, filt);
            int wrap = (int)GLUtils.ToGLWrapParameter(spec.Wrap);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, wrap);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, wrap);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, wrap);

            gl.FramebufferTexture2D(GLEnum.Framebuffer, GLEnum.ColorAttachment0 + index, GLEnum.Texture2D, texid, 0);
        }

        private static unsafe void AttachDepthTexture(uint texid, FramebufferTextureSpecification spec, uint width, uint height)
        {
            gl.TexImage2D(
                GLEnum.Texture2D,
                0,
                (int)GLUtils.ToGLTextureFormat(spec.Format),
                width,
                height,
                0,
                Utils.GetColorFormat(spec.Format),
                GLEnum.UnsignedInt248, // we need to care about the type or opengl will get mad
                null);

            int filt = (int)GLUtils.ToGLFilterParameter(spec.Filter);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, filt);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, filt);
            int wrap = (int)GLUtils.ToGLWrapParameter(spec.Wrap);
            //glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_R, wrap);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, wrap);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, wrap);

            gl.FramebufferTexture2D(GLEnum.Framebuffer, Utils.GetAttachmentType(spec.Format), GLEnum.Texture2D, texid, 0);
        }

        public override void BlitTo(WeakReference<Framebuffer> target)
        {
            throw new NotImplementedException();
            gl.BindFramebuffer(GLEnum.ReadFramebuffer, this._rendererId);
            gl.BindFramebuffer(GLEnum.DrawFramebuffer, target == null ? 0 : ((GLFramebuffer)target.GetValue())._rendererId);
            gl.BlitFramebuffer(0, 0, (int)Width, (int)Height, 0, 0, (int)Width, (int)Height, (uint)GLEnum.DepthBufferBit, GLEnum.Nearest);
            gl.BindFramebuffer(GLEnum.Framebuffer, 0);
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
