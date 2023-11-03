using GLEnum = Silk.NET.OpenGL.GLEnum;
using System;
using System.Collections.Generic;
using System.Numerics;

using CrossEngine.Profiling;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Shaders;
using static CrossEngine.Platform.OpenGL.GLContext;
using CrossEngine.Debugging;
using CrossEngine.Utils;
using System.Diagnostics;

namespace CrossEngine.Platform.OpenGL
{
    internal class GLShaderProgram : ShaderProgram
    {
        private struct ShaderParameter : ShaderProgram.IShaderParameter
        {
            public ShaderDataType Type => this.type;
            public string Name => this.name;

            public ShaderDataType type;
            public string name;
            internal int location;
            internal uint programid;

            public ShaderParameter(ShaderDataType type, string name, int location, uint programid)
            {
                this.type = type;
                this.name = name;
                this.location = location;
                this.programid = programid;
            }
        }

        private uint _rendererId;

        private Dictionary<string, int> _uniformLocationCache = new Dictionary<string, int> { }; // for optimalization
        private readonly Dictionary<string, IShaderParameter> _attributes = new Dictionary<string, IShaderParameter>();
        private readonly Dictionary<string, IShaderParameter> _uniforms = new Dictionary<string, IShaderParameter>();

        public override IReadOnlyDictionary<string, IShaderParameter> Attributes { get; protected set; }
        public override IReadOnlyDictionary<string, IShaderParameter> Uniforms { get; protected set; }

        GLShaderProgram()
        {
            Profiler.Function();

            _rendererId = gl.CreateProgram();

            Attributes = _attributes;
            Uniforms = _uniforms;

            GC.KeepAlive(this);
            GPUGC.Register(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} created (id: {_rendererId})");
        }

        public GLShaderProgram(GLShader vertex, GLShader fragment) : this()
        {
            if (vertex.Type != ShaderType.Vertex || fragment.Type != ShaderType.Fragment) throw new InvalidOperationException();

            gl.AttachShader(_rendererId, vertex.RendererId);
            gl.AttachShader(_rendererId, fragment.RendererId);

            gl.LinkProgram(_rendererId);
            bool linkSuccess = !CheckLinkingErrors();

            // cleanup
            gl.DetachShader(_rendererId, vertex.RendererId);
            gl.DetachShader(_rendererId, fragment.RendererId);

            if (!linkSuccess) return;

            GetParameters();
        }

        public GLShaderProgram(GLShader vertex, GLShader fragment, GLShader geometry) : this()
        {
            if (vertex.Type != ShaderType.Vertex || fragment.Type != ShaderType.Fragment || geometry.Type != ShaderType.Geometry) throw new InvalidOperationException();

            gl.AttachShader(_rendererId, vertex.RendererId);
            gl.AttachShader(_rendererId, fragment.RendererId);
            gl.AttachShader(_rendererId, geometry.RendererId);

            gl.LinkProgram(_rendererId);
            bool linkSuccess = !CheckLinkingErrors();

            // cleanup
            gl.DetachShader(_rendererId, vertex.RendererId);
            gl.DetachShader(_rendererId, fragment.RendererId);
            gl.DetachShader(_rendererId, geometry.RendererId);

            if (!linkSuccess) return;

            GetParameters();
        }

        protected override void Dispose(bool disposing)
        {
            Profiler.Function();

            if (Disposed)
                return;

            if (disposing)
            {
                // free any other managed objects here
            }

            // free any unmanaged objects here
            gl.DeleteProgram(_rendererId);

            GC.ReRegisterForFinalize(this);
            GPUGC.Unregister(this);

            RendererAPI.Log.Trace($"{this.GetType().Name} deleted (id: {_rendererId})");

            Disposed = true;
        }

        public override void Use()
        {
            Profiler.Function();

            gl.UseProgram(_rendererId);
        }

        public override void Disuse()
        {
            Profiler.Function();

            gl.UseProgram(0);
        }

        #region Uniforms
        /*
        public unsafe void SetUniform(string name, object value)
        {
            switch (_uniforms[name].Type)
            {
                case ShaderDataType.Float:
                    {
                        glUniform1f(_uniforms[name].Location, (float)value);
                    }
                    return;
                case ShaderDataType.Float2:
                    {
                        Vector2 v = (Vector2)value;
                        glUniform2f(_uniforms[name].Location, v.X, v.Y);
                    }
                    return;
                case ShaderDataType.Float3:
                    {
                        Vector3 v = (Vector3)value;
                        glUniform3f(_uniforms[name].Location, v.X, v.Y, v.Z);
                    }
                    return;
                case ShaderDataType.Float4:
                    {
                        Vector4 v = (Vector4)value;
                        glUniform4f(_uniforms[name].Location, v.X, v.Y, v.Z, v.W);
                    }
                    return;
                case ShaderDataType.Mat3:
                    {
                        throw new NotImplementedException();
                    }
                    return;
                case ShaderDataType.Mat4:
                    {
                        Matrix4x4 v = ((Matrix4x4)value);
                        glUniformMatrix4fv(_uniforms[name].Location, 1, false, &v.M11);
                    }
                    return;
                case ShaderDataType.Int:
                    {
                        glUniform1i(_uniforms[name].Location, (int)value);
                    }
                    return;
                case ShaderDataType.Int2:
                    {
                        IntVec2 v = (IntVec2)value;
                        glUniform2i(_uniforms[name].Location, v.X, v.Y);
                    }
                    return;
                case ShaderDataType.Int3:
                    {
                        IntVec3 v = (IntVec3)value;
                        glUniform3i(_uniforms[name].Location, v.X, v.Y, v.Z);
                    }
                    return;
                case ShaderDataType.Int4:
                    {
                        throw new NotImplementedException();
                    }
                    return;
                case ShaderDataType.Bool:
                    {
                        glUniform1i(_uniforms[name].Location, (bool)value ? 1 : 0);
                    }
                    return;
            }

            Debug.Assert(false, "Uniform does not exist");
        }
        */

        // ##### simple #####

        public override void SetParameter1(string name, float value)
        {
            gl.Uniform1(GetUniformLocation(name), value);
        }

        public override void SetParameter1(string name, int value)
        {
            gl.Uniform1(GetUniformLocation(name), value);
        }

        // ##### vec2 #####

        public override void SetParameter2(string name, float x, float y)
        {
            gl.Uniform2(GetUniformLocation(name), x, y);
        }

        // ##### vec3 #####

        public override void SetParameter3(string name, float x, float y, float z)
        {
            gl.Uniform3(GetUniformLocation(name), x, y, z);
        }

        // ##### vec4 #####

        public override void SetParameter4(string name, float x, float y, float z, float w)
        {
            gl.Uniform4(GetUniformLocation(name), x, y, z, w);
        }

        // ##### matricies #####

        public override unsafe void SetParameterMat4(string name, Matrix4x4 mat)
        {
            gl.UniformMatrix4(GetUniformLocation(name), 1, false, &mat.M11);
        }

        // ##### other #####

        public override unsafe void SetParameter1(string name, int[] intVec)
        {
            Debug.Assert(intVec.Length > 0);

            fixed (int* ivp = &intVec[0])
                gl.Uniform1(GetUniformLocation(name), (uint)intVec.Length, ivp);
        }
        #endregion

        private int GetUniformLocation(string name)
        {
            if (_uniformLocationCache.ContainsKey(name))
                return _uniformLocationCache[name];

            int location = gl.GetUniformLocation(_rendererId, name);
            if (location != -1)
                _uniformLocationCache.Add(name, location);
            else
                RendererAPI.Log.Warn($"no uniform named '{name}' found");

            return location;
        }

        // true if error found
        private unsafe bool CheckLinkingErrors()
        {
            GLEnum status = 0;
            gl.GetProgram(_rendererId, GLEnum.LinkStatus, (int*)&status);
            if (status == GLEnum.False)
            {
                RendererAPI.Log.Error("Shader program linking failed!\n" + gl.GetProgramInfoLog(_rendererId));

                return true;
            }
            return false;
        }

        private unsafe void GetParameters()
        {
            gl.UseProgram(_rendererId);

            int attributeCount;
            gl.GetProgram(_rendererId, GLEnum.ActiveAttributes, &attributeCount);
            int attributeMaxLength;
            gl.GetProgram(_rendererId, GLEnum.ActiveAttributeMaxLength, &attributeMaxLength);
            Debug.Assert((MaxParamNameLength != null) ? attributeMaxLength <= MaxParamNameLength : true, "Parameter maximal name length exceeded");
            for (uint i = 0; i < attributeCount; i++)
            {
                gl.GetActiveAttrib(_rendererId, i, (uint)attributeMaxLength, out uint length, out int size, out GLEnum type, out string name);
                int location = gl.GetAttribLocation(_rendererId, name);

                var attribute = new ShaderParameter(GLUtils.ToShaderDataType(type), name, location, _rendererId);
                _attributes.Add(name, attribute);

                RendererAPI.Log.Trace("shader attribute parameter: {0} {1}", attribute.Type, attribute.Name);
            }

            int uniformCount;
            gl.GetProgram(_rendererId, GLEnum.ActiveUniforms, &uniformCount);
            int uniformMaxLength;
            gl.GetProgram(_rendererId, GLEnum.ActiveUniformMaxLength, &uniformMaxLength);
            Debug.Assert((MaxParamNameLength != null) ? uniformMaxLength <= MaxParamNameLength : true, "Parameter maximal name length exceeded");
            for (uint i = 0; i < uniformCount; i++)
            {
                gl.GetActiveUniform(_rendererId, i, (uint)uniformMaxLength, out uint length, out int size, out GLEnum type, out string name);
                int location = gl.GetUniformLocation(_rendererId, name);

                var uniform = new ShaderParameter(GLUtils.ToShaderDataType(type), name, location, _rendererId);
                _uniforms.Add(name, uniform);

                RendererAPI.Log.Trace("shader uniform parameter: {0} {1}", uniform.Type, uniform.Name);
            }
        }
    }
}
