using System;
using GLEnum = Silk.NET.OpenGL.GLEnum;

using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Shaders;
using static CrossEngine.Platform.OpenGL.GLContext;
using CrossEngine.Debugging;

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

            RendererAPI.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        protected override void Dispose(bool disposing)
        {
            Profiler.Function();

            if (Disposed)
                return;

            gl.DeleteShader(_rendererId);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        // true if error found
        private unsafe bool CheckCompileErrors()
        {
            GLEnum compiled = 0;
            gl.GetShader(_rendererId, GLEnum.CompileStatus, (int*)&compiled);
            if (compiled == GLEnum.False)
            {
                int length = 0;
                
                char[] infoLog = new char[length];
                RendererAPI.Log.Error($"{Type} shader compilation failed!\n" + gl.GetShaderInfoLog(_rendererId));

                return true;
            }
            return false;
        }
    }
}
