using System;

using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Shaders;
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
    internal class GLShader : Shader
    {
        uint _rendererId;

        public uint RendererId => _rendererId;

        public bool Disposed { get; protected set; } = false;

        public GLShader(string source, ShaderType type) : base(type)
        {
            Profiler.Function();

            _rendererId = gl.CreateShader(GLUtils.ToGLShaderType(Type));
            gl.ShaderSource(_rendererId, source);
            gl.CompileShader(_rendererId);

            CheckCompileErrors();

            RendererApi.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        protected override void Dispose(bool disposing)
        {
            Profiler.Function();

            if (Disposed)
                return;

            gl.DeleteShader(_rendererId);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererApi.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        // true if error found
        private unsafe bool CheckCompileErrors()
        {
            GLEnum compiled = 0;
            gl.GetShader(_rendererId, GLEnum.CompileStatus, (int*)&compiled);
            if (compiled == GLEnum.False)
            {
                uint length = 0;
                gl.GetShader(_rendererId, GLEnum.InfoLogLength, (int*)&length);
                byte[] infoLog = new byte[length];
                string message;
                fixed (byte* p = infoLog)
                {
                    gl.GetShaderInfoLog(_rendererId, length, &length, p);
                    message = GLHelper.PtrToStringUtf8((IntPtr)p);
                }
                RendererApi.Log.Error($"{Type} shader compilation failed!\n" + message);
                return true;
            }
            return false;
        }
    }
}
