using System;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Logging;
using CrossEngine.Profiling;
using CrossEngine.Utils;

using CrossEngine.Platform.OpenGL;

namespace CrossEngine.Rendering.Shaders
{
    public abstract class ShaderProgram : IDisposable
    {
        //Shader VertexShader;
        //Shader FragmentShader;
        //Shader GeomtryShader;

        protected static int? MaxParamNameLength = 256;

        public interface IShaderParameter
        {
            public ShaderDataType Type { get; }
            public string Name { get; }
        }

        public virtual IReadOnlyDictionary<string, IShaderParameter> Attributes
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }
        public virtual IReadOnlyDictionary<string, IShaderParameter> Uniforms
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

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

        ~ShaderProgram()
        {
            Dispose(false);
        }

        public static WeakReference<ShaderProgram> Create(Shader vertex, Shader fragment)
        {
            return Create(new WeakReference<ShaderProgram>(null), vertex, fragment);
        }
        
        public static WeakReference<ShaderProgram> Create(WeakReference<ShaderProgram> program, Shader vertex, Shader fragment)
        {
            switch (RendererApi.GetApi())
            {
                case GraphicsApi.None: Debug.Assert(false, $"No API is not supported"); return null;
                case GraphicsApi.OpenGLES:
                case GraphicsApi.OpenGL: return new WeakReference<ShaderProgram>(new GLShaderProgram((GLShader)vertex, (GLShader)fragment));
            }

            Debug.Assert(false, $"Udefined {nameof(GraphicsApi)} value");
            return null;
        }

        public abstract void Use();
        public abstract void Disuse();

        // TODO: add more and polish
        #region Uniforms
        public abstract void SetParameterFloat(string name, float value);
        public abstract void SetParameterInt(string name, int value);
        
        public abstract void SetParameterVec2(string name, float x, float y);
        public void SetParameterVec2(string name, Vector2 vec) => SetParameterVec2(name, vec.X, vec.Y);

        public abstract void SetParameterVec3(string name, float x, float y, float z);
        public void SetParameterVec3(string name, Vector3 vec) => SetParameterVec3(name, vec.X, vec.Y, vec.Z);

        public abstract void SetParameterVec4(string name, float x, float y, float z, float w);
        public void SetParameterVec4(string name, Vector4 vec) => SetParameterVec4(name, vec.X, vec.Y, vec.Z, vec.W);

        public abstract void SetParameterMat4(string name, Matrix4x4 mat);

        public abstract void SetParameterIntVec(string name, int[] intVec);
        #endregion
    }
}
