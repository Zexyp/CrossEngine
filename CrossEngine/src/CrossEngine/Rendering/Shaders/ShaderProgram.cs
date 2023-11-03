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
            switch (RendererAPI.GetAPI())
            {
                case RendererAPI.API.None: Debug.Assert(false, $"No API is not supported"); return null;
                case RendererAPI.API.OpenGL: return new WeakReference<ShaderProgram>(new GLShaderProgram((GLShader)vertex, (GLShader)fragment));
            }

            Debug.Assert(false, $"Udefined {nameof(RendererAPI.API)} value");
            return null;
        }

        public abstract void Use();
        public abstract void Disuse();

        // TODO: add more and polish
        #region Uniforms
        public abstract void SetParameter1(string name, float value);
        public abstract void SetParameter1(string name, int value);
        
        public abstract void SetParameter2(string name, float x, float y);
        public void SetParameter2(string name, Vector2 vec) => SetParameter2(name, vec.X, vec.Y);

        public abstract void SetParameter3(string name, float x, float y, float z);
        public void SetParameter3(string name, Vector3 vec) => SetParameter3(name, vec.X, vec.Y, vec.Z);

        public abstract void SetParameter4(string name, float x, float y, float z, float w);
        public void SetParameter4(string name, Vector4 vec) => SetParameter4(name, vec.X, vec.Y, vec.Z, vec.W);

        public abstract void SetParameterMat4(string name, Matrix4x4 mat);

        public abstract void SetParameter1(string name, int[] intVec);
        #endregion
    }
}
