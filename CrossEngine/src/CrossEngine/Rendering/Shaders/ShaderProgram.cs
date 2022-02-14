using System;
using static OpenGL.GL;

using System.Collections.Generic;
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
    public class ShaderProgram : IDisposable
    {
        //Shader VertexShader;
        //Shader FragmentShader;
        //Shader GeomtryShader;

        static int? MaxParamNameLength = 256;

        public struct AttributeParameter
        {
            public ShaderDataType Type;
            public string Name;
            internal int Location;
            internal uint programid;
        
            public AttributeParameter(ShaderDataType type, string name, int location, uint programid)
            {
                this.Type = type;
                this.Name = name;
                this.Location = location;
                this.programid = programid;
            }
        }
        public struct UniformParameter
        {
            public ShaderDataType Type;
            public string Name;
            internal int Location;
            internal uint programid;
        
            public UniformParameter(ShaderDataType type, string name, int location, uint programid)
            {
                this.Type = type;
                this.Name = name;
                this.Location = location;
                this.programid = programid;
            }
        }

        private readonly Dictionary<string, AttributeParameter> _attributes = new Dictionary<string, AttributeParameter>();
        private readonly Dictionary<string, UniformParameter> _uniforms = new Dictionary<string, UniformParameter>();
        public readonly IReadOnlyCollection<AttributeParameter> Attributes;
        public readonly IReadOnlyCollection<UniformParameter> Uniforms;

        uint _rendererId;

        Dictionary<string, int> _uniformLocationCache = new Dictionary<string, int> { }; // for optimalization

        public bool Disposed { get; protected set; } = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Profiler.Function();

            if (Disposed)
                return;

            if (disposing)
            {
                // free any other managed objects here
            }

            // free any unmanaged objects here
            glDeleteProgram(_rendererId);

            Disposed = true;
        }

        ~ShaderProgram()
        {
            Dispose(false);
        }

        ShaderProgram()
        {
            Profiler.Function();

            _rendererId = glCreateProgram();

            Attributes = _attributes.Values;
            Uniforms = _uniforms.Values;
        }

        public ShaderProgram(Shader vertex, Shader fragment) : this()
        {
            if (vertex.Type != ShaderType.Vertex || fragment.Type != ShaderType.Fragment) throw new InvalidOperationException();

            glAttachShader(_rendererId, vertex.RendererId);
            glAttachShader(_rendererId, fragment.RendererId);

            glLinkProgram(_rendererId);
            bool linkSuccess = !CheckLinkingErrors();

            // cleanup
            glDetachShader(_rendererId, vertex.RendererId);
            glDetachShader(_rendererId, fragment.RendererId);

            if (!linkSuccess) return;

            GetParameters();
        }

        public void Use()
        {
            Profiler.Function();

            glUseProgram(_rendererId);
        }

        public static void Disuse()
        {
            Profiler.Function();

            glUseProgram(0);
        }

        // TODO: add more and polish
        #region Uniforms
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

        // ##### simple #####

        public void SetFloat(string name, float value)
        {
            glUniform1f(GetUniformLocation(name), value);
        }

        public void SetInt(string name, int value)
        {
            glUniform1i(GetUniformLocation(name), value);
        }

        // ##### vec2 #####

        public void SetVec2(string name, float x, float y)
        {
            glUniform2f(GetUniformLocation(name), x, y);
        }

        public void SetVec2(string name, Vector2 vec)
        {
            glUniform2f(GetUniformLocation(name), vec.X, vec.Y);
        }

        // ##### vec3 #####

        public void SetVec3(string name, float x, float y, float z)
        {
            glUniform3f(GetUniformLocation(name), x, y, z);
        }

        public void SetVec3(string name, Vector3 vec)
        {
            glUniform3f(GetUniformLocation(name), vec.X, vec.Y, vec.Z);
        }

        // ##### vec4 #####

        public void SetVec4(string name, float x, float y, float z, float w)
        {
            glUniform4f(GetUniformLocation(name), x, y, z, w);
        }

        public void SetVec4(string name, Vector4 vec)
        {
            glUniform4f(GetUniformLocation(name), vec.X, vec.Y, vec.Z, vec.W);
        }

        // ##### matricies #####

        public unsafe void SetMat4(string name, Matrix4x4 mat)
        {
            glUniformMatrix4fv(GetUniformLocation(name), 1, false, &mat.M11);
        }

        // ##### other #####

        public unsafe void SetIntVec(string name, int[] intVec)
        {
            Debug.Assert(intVec.Length > 0);

            fixed (int* ivp = &intVec[0])
                glUniform1iv(GetUniformLocation(name), intVec.Length, ivp);
        }
        #endregion

        private int GetUniformLocation(string name)
        {
            if (_uniformLocationCache.ContainsKey(name))
                return _uniformLocationCache[name];

            int location = glGetUniformLocation(_rendererId, name);
            if (location != -1)
                _uniformLocationCache.Add(name, location);
            else
                Log.Core.Warn($"no uniform named '{name}' found");

            return location;
        }

        // true if error found
        private unsafe bool CheckLinkingErrors()
        {
            int status = 0;
            glGetProgramiv(_rendererId, GL_LINK_STATUS, &status);
            if (status == GL_FALSE)
            {
                int length = 0;
                glGetProgramiv(_rendererId, GL_INFO_LOG_LENGTH, &length);

                char[] infoLog = new char[length];
                Log.Core.Error("Shader program linking failed!\n" + glGetProgramInfoLog(_rendererId, length));

                return true;
            }
            return false;
        }

        private unsafe void GetParameters()
        {
            glUseProgram(_rendererId);
        
            int attributeCount;
            glGetProgramiv(_rendererId, GL_ACTIVE_ATTRIBUTES, &attributeCount);
            int attributeMaxLength;
            glGetProgramiv(_rendererId, GL_ACTIVE_ATTRIBUTE_MAX_LENGTH, &attributeMaxLength);
            Debug.Assert((MaxParamNameLength != null) ? attributeMaxLength <= MaxParamNameLength : true, "Max parameter name length exceeded");
            for (uint i = 0; i < attributeCount; i++)
            {
                glGetActiveAttrib(_rendererId, i, attributeMaxLength, out int length, out int size, out int type, out string name);
                int location = glGetAttribLocation(_rendererId, name);

                var attribute = new AttributeParameter(GLUtils.ToShaderDataType(type), name, location, _rendererId);
                _attributes.Add(name, attribute);
        
                Log.Core.Trace("shader attribute parameter: {0} {1}", attribute.Type, attribute.Name);
            }
        
            int uniformCount;
            glGetProgramiv(_rendererId, GL_ACTIVE_UNIFORMS, &uniformCount);
            int uniformMaxLength;
            glGetProgramiv(_rendererId, GL_ACTIVE_UNIFORM_MAX_LENGTH, &uniformMaxLength);
            Debug.Assert((MaxParamNameLength != null) ? uniformMaxLength <= MaxParamNameLength : true, "Max parameter name length exceeded");
            for (uint i = 0; i < uniformCount; i++)
            {
                glGetActiveUniform(_rendererId, i, uniformMaxLength, out int length, out int size, out int type, out string name);
                int location = glGetUniformLocation(_rendererId, name);

                var uniform = new UniformParameter(GLUtils.ToShaderDataType(type), name, location, _rendererId);
                _uniforms.Add(name, uniform);
        
                Log.Core.Trace("shader uniform parameter: {0} {1}", uniform.Type, uniform.Name);
            }
        }
    }
}
