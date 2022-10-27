using System;
using static OpenGL.GL;

using System.Diagnostics;

using CrossEngine.Profiling;
using CrossEngine.Rendering.Textures;
using CrossEngine.Rendering;

namespace CrossEngine.Platform.OpenGL
{
    class GLTexture : Texture
    {
        internal uint _rendererId;
        private int _filtering = GL_LINEAR;
        private uint _width, _height;
        private int _internalFormat, _dataFormat;

        public override uint RendererId => _rendererId;
        public override uint Width => _width;
        public override uint Height => _height;

        unsafe GLTexture()
        {
            Profiler.Function();

            fixed (uint* p = &_rendererId)
                glGenTextures(1, p);

            RendererAPI.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
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
                glDeleteTextures(1, p);

            RendererAPI.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        public override void Bind(uint slot = 0)
        {
            glActiveTexture(GL_TEXTURE0 + (int)slot);
            glBindTexture(GLUtils.ToGLTextureTarget(Target), _rendererId);
        }

        public override void Unbind()
        {
            glBindTexture(GLUtils.ToGLTextureTarget(Target), 0);
        }

        public override unsafe void SetData(void* data, uint size)
        {
            int bpp = 0;
            switch (_internalFormat)
            {
                case GL_RGB: bpp = 3; break;
                case GL_RGBA: bpp = 4; break;
            }
            Debug.Assert(size == _width * _height * bpp);

            int gltarg = GLUtils.ToGLTextureTarget(Target);
            glBindTexture(gltarg, _rendererId);
            glTexSubImage2D(gltarg, 0, 0, 0, (int)_width, (int)_height, _dataFormat, GL_UNSIGNED_BYTE, data);
        }

        public unsafe void SetData(void* data, uint width, uint height, ColorFormat suppliedFormat, ColorFormat internalFormat)
        {
            Target = TextureTarget.Texture2D;
            int gltarg = GLUtils.ToGLTextureTarget(Target);

            glBindTexture(gltarg, _rendererId);

            // TODO: add data type
            _internalFormat = GLUtils.ToGLColorFormat(internalFormat);
            _dataFormat = GLUtils.ToGLColorFormat(suppliedFormat);
            glTexImage2D(gltarg, 0, _internalFormat, (int)width, (int)height, 0, _dataFormat, GL_UNSIGNED_BYTE, data);

            // ! this should be used
            //if (generateMipmaps) glGenerateMipmap((int)_target);

            // parameters need to be set
            glTexParameteri(gltarg, GL_TEXTURE_MIN_FILTER, _filtering);
            glTexParameteri(gltarg, GL_TEXTURE_MAG_FILTER, _filtering);

            _width = width;
            _height = height;
        }

        public override void SetFilterParameter(FilterParameter filter)
        {
            int glfilt = GLUtils.ToGLFilterParameter(filter);
            int gltarg = GLUtils.ToGLTextureTarget(Target);
            glTexParameteri(gltarg, GL_TEXTURE_MIN_FILTER, glfilt);
            glTexParameteri(gltarg, GL_TEXTURE_MAG_FILTER, glfilt);

            _filtering = glfilt;
        }

        public override void SetWrapParameter(WrapParameter wrap)
        {
            int glwrap = GLUtils.ToGLWrapParameter(wrap);
            int gltarg = GLUtils.ToGLTextureTarget(Target);
            glTexParameteri(gltarg, GL_TEXTURE_WRAP_R, glwrap); // for 3d texture
            glTexParameteri(gltarg, GL_TEXTURE_WRAP_S, glwrap);
            glTexParameteri(gltarg, GL_TEXTURE_WRAP_T, glwrap);
        }
    }
}
