using System;

using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Rendering.Textures;
using CrossEngine.Rendering;
using CrossEngine.Debugging;

#if WASM
using GLEnum = Silk.NET.OpenGLES.GLEnum;
using static CrossEngine.Platform.Wasm.EGLContext;
#else
using GLEnum = Silk.NET.OpenGL.GLEnum;
using static CrossEngine.Platform.OpenGL.GLContext;
#endif

namespace CrossEngine.Platform.OpenGL
{
    class GLTexture : Texture
    {
        internal uint _rendererId;
        private int _filtering = (int)GLEnum.Linear;
        private uint _width, _height;
        private GLEnum _dataFormat;
        private int _internalFormat;

        public override uint RendererId => _rendererId;
        public override uint Width => _width;
        public override uint Height => _height;

        unsafe GLTexture()
        {
            Profiler.Function();

            fixed (uint* p = &_rendererId)
                gl.GenTextures(1, p);

            GC.KeepAlive(this);
            GPUGC.Register(this);

            RendererApi.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        public unsafe GLTexture(uint width, uint height, ColorFormat internalFormat) : this()
        {
            SetData(null, width, height, internalFormat, internalFormat);
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
                gl.DeleteTextures(1, p);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererApi.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        public override void Bind(uint slot = 0)
        {
            gl.ActiveTexture(GLEnum.Texture0 + (int)slot);
            gl.BindTexture(GLUtils.ToGLTextureTarget(Target), _rendererId);
        }

        public override void Unbind()
        {
            gl.BindTexture(GLUtils.ToGLTextureTarget(Target), 0);
        }

        public override unsafe void SetData(void* data, uint size)
        {
            int bpp = 0;
            switch ((GLEnum)_internalFormat)
            {
                case GLEnum.Rgb: bpp = 3; break;
                case GLEnum.Rgba: bpp = 4; break;
            }
            Debug.Assert(size == _width * _height * bpp);

            GLEnum gltarg = GLUtils.ToGLTextureTarget(Target);
            gl.BindTexture(gltarg, _rendererId);
            gl.TexSubImage2D(gltarg, 0, 0, 0, _width, _height, _dataFormat, GLEnum.UnsignedByte, data);
        }

        public unsafe void SetData(void* data, uint width, uint height, ColorFormat suppliedFormat, ColorFormat internalFormat)
        {
            Target = TextureTarget.Texture2D;
            GLEnum gltarg = GLUtils.ToGLTextureTarget(Target);

            gl.BindTexture(gltarg, _rendererId);

            // TODO: add data type
            _internalFormat = (int)GLUtils.ToGLColorFormat(internalFormat);
            _dataFormat = GLUtils.ToGLColorFormat(suppliedFormat);
            gl.TexImage2D(gltarg, 0, _internalFormat, width, height, 0, _dataFormat, GLEnum.UnsignedByte, data);

            // ! this should be used
            //if (generateMipmaps) glGenerateMipmap((int)_target);

            // parameters need to be set
            gl.TexParameter(gltarg, GLEnum.TextureMinFilter, _filtering);
            gl.TexParameter(gltarg, GLEnum.TextureMagFilter, _filtering);

            _width = width;
            _height = height;
        }

        public override void SetFilterParameter(FilterParameter filter)
        {
            int glfilt = (int)GLUtils.ToGLFilterParameter(filter);
            GLEnum gltarg = GLUtils.ToGLTextureTarget(Target);
            gl.TexParameter(gltarg, GLEnum.TextureMinFilter, glfilt);
            gl.TexParameter(gltarg, GLEnum.TextureMagFilter, glfilt);

            _filtering = glfilt;
        }

        public override void SetWrapParameter(WrapParameter wrap)
        {
            int glwrap = (int)GLUtils.ToGLWrapParameter(wrap);
            GLEnum gltarg = GLUtils.ToGLTextureTarget(Target);
            //gl.TexParameter(gltarg, GLEnum.TextureWrapR, glwrap); // for 3d texture
            gl.TexParameter(gltarg, GLEnum.TextureWrapS, glwrap);
            gl.TexParameter(gltarg, GLEnum.TextureWrapT, glwrap);
        }
    }
}
