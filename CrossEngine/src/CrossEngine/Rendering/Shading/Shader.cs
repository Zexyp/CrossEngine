using System;
using static OpenGL.GL;

using System.Numerics;
using System.Collections.Generic;

using CrossEngine.Logging;

namespace CrossEngine.Rendering.Shaders
{
    public class Shader : IDisposable
    {
        #region Parameter Structs
        //struct AttributeParameter
        //{
        //    ShaderDataType type;
        //    string name;
        //    int location;
        //    uint programid;
        //
        //    public AttributeParameter(ShaderDataType type, string name, int location, uint programid)
        //    {
        //        this.type = type;
        //        this.name = name;
        //        this.location = location;
        //        this.programid = programid;
        //    }
        //}
        //struct UniformParameter
        //{
        //    ShaderDataType type;
        //    string name;
        //    int location;
        //    uint programid;
        //
        //    public UniformParameter(ShaderDataType type, string name, int location, uint programid)
        //    {
        //        this.type = type;
        //        this.name = name;
        //        this.location = location;
        //        this.programid = programid;
        //    }
        //}
        #endregion

        const int MaxParamNameLength = 256;

        private uint _id = 0;

        public uint ID { get => _id; }

        Dictionary<string, int> uniformLocationCache = new Dictionary<string, int> { }; // for optimalization

        //Dictionary<string, AttributeParameter> attributes;
        //Dictionary<string, UniformParameter> uniforms;

        public Shader(string vertexSource, string fragmentSource)
        {
            uint vertex = 0;
            uint fragment = 0;

            // vertex
            vertex = glCreateShader(GL_VERTEX_SHADER);
            glShaderSource(vertex, vertexSource);
            glCompileShader(vertex);

            CheckCompileErrors(vertex);

            // fragment
            fragment = glCreateShader(GL_FRAGMENT_SHADER);
            glShaderSource(fragment, fragmentSource);
            glCompileShader(fragment);

            CheckCompileErrors(fragment);

            // linking
            _id = glCreateProgram();
            Log.Core.Trace("shader created (id: {0})", _id);
            glAttachShader(_id, vertex);
            glAttachShader(_id, fragment);

            glLinkProgram(_id);

            if (CheckLinkingErrors(_id)) Dispose();

            // cleanup
            glDetachShader(_id, vertex);
            glDetachShader(_id, fragment);

            glDeleteShader(vertex);
            glDeleteShader(fragment);


            //GetParameters();
        }

        //private unsafe void GetParameters()
        //{
        //    glUseProgram(_id);
        //
        //    int attributeCount;
        //    glGetProgramiv(_id, GL_ACTIVE_ATTRIBUTES, &attributeCount);
        //
        //    for (uint i = 0; i < attributeCount; i++)
        //    {
        //        glGetActiveAttrib(_id, i, MaxParamNameLength, out int length, out int size, out int type, out string name);
        //        int location = glGetAttribLocation(_id, name);
        //
        //        attributes.Add(name, new AttributeParameter((ShaderDataType)type, name, location, _id));
        //
        //        Log.Core.Trace("shader attribute parameter: {0} {1}", ((ShaderDataType)type).ToString(), name);
        //    }
        //
        //    int uniformCount;
        //    glGetProgramiv(_id, GL_ACTIVE_UNIFORMS, &uniformCount);
        //
        //    for (uint i = 0; i < uniformCount; i++)
        //    {
        //        glGetActiveUniform(_id, i, MaxParamNameLength, out int length, out int size, out int type, out string name);
        //        int location = glGetUniformLocation(_id, name);
        //
        //        uniforms.Add(name, new UniformParameter((ShaderDataType)type, name, location, _id));
        //
        //        Log.Core.Trace("shader uniform parameter: {0} {1}", ((ShaderDataType)type).ToString(), name);
        //    }
        //}

        #region Disposure
        // cleanup
        ~Shader()
        {
            Log.Core.Error("unhandled shader disposure (id: {0})", _id);
            System.Diagnostics.Debug.Assert(false);

            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (_id != 0)
            {
                Log.Core.Trace("deleting shader (id: {0})", _id);

                // make sure that the program isn't used right now
                int usedid;
                glGetIntegerv(GL_CURRENT_PROGRAM, &usedid);
                if (usedid == _id) glUseProgram(0);

                glDeleteProgram(_id);

                _id = 0;
            }
        }
        #endregion

        #region Error Checks
        // true if error found
        private unsafe bool CheckCompileErrors(uint shader)
        {
            int compiled = 0;
            glGetShaderiv(shader, GL_COMPILE_STATUS, &compiled);
            if (compiled == GL_FALSE)
            {
                int length = 0;
                glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &length);

                char[] infoLog = new char[length];
                Log.Core.Error("shader compilation failed!\n" + glGetShaderInfoLog(shader, length));

                glDeleteShader(shader);

                return true;
            }
            return false;
        }

        private unsafe bool CheckLinkingErrors(uint program)
        {
            int status = 0;
            glGetProgramiv(program, GL_LINK_STATUS, &status);
            if (status == GL_FALSE)
            {
                int length = 0;
                glGetProgramiv(program, GL_INFO_LOG_LENGTH, &length);

                char[] infoLog = new char[length];
                Log.Core.Error("shader linking failed!\n" + glGetProgramInfoLog(program, length));

                glDeleteProgram(program);

                // to reset the id
                _id = 0;
                
                return true;
            }
            return false;
        }
        #endregion

        public void Use()
        {
            glUseProgram(_id);
        }

        public static void Disuse()
        {
            glUseProgram(0);
        }

        // uniforms
        #region Uniforms
        public void SetFloat(string name, float value)
        {
            glUniform1f(GetUniformLocation(name), value);
        }
        public void SetInt(string name, int value)
        {
            glUniform1i(GetUniformLocation(name), value);
        }

        // vec2

        public void SetVec2(string name, float x, float y)
        {
            glUniform2f(GetUniformLocation(name), x, y);
        }

        public void SetVec2(string name, Vector2 vec)
        {
            glUniform2f(GetUniformLocation(name), vec.X, vec.Y);
        }

        // vec3

        public void SetVec3(string name, float x, float y, float z)
        {
            glUniform3f(GetUniformLocation(name), x, y, z);
        }

        public void SetVec3(string name, Vector3 vec)
        {
            glUniform3f(GetUniformLocation(name), vec.X, vec.Y, vec.Z);
        }

        public void SetVec3(string name, System.Drawing.Color color) => SetVec3(name, new Vector3((float)color.R / byte.MaxValue, (float)color.G / byte.MaxValue, (float)color.B / byte.MaxValue));

        // vec4

        public void SetVec4(string name, float x, float y, float z, float w)
        {
            glUniform4f(GetUniformLocation(name), x, y, z, w);
        }

        public void SetVec4(string name, Vector4 vec)
        {
            glUniform4f(GetUniformLocation(name), vec.X, vec.Y, vec.Z, vec.W);
        }

        public void SetVec4(string name, System.Drawing.Color color) => SetVec4(name, new Vector4((float)color.R / byte.MaxValue, (float)color.G / byte.MaxValue, (float)color.B / byte.MaxValue, (float)color.A / byte.MaxValue));

        public unsafe void SetMat4(string name, Matrix4x4 mat)
        {
            glUniformMatrix4fv(GetUniformLocation(name), 1, false, &mat.M11);
        }

        public unsafe void SetIntVec(string name, int[] intVec)
        {
            if(intVec.Length > 0)
            {
                fixed (int* ivp = &intVec[0])
                    glUniform1iv(GetUniformLocation(name), intVec.Length, ivp);
            }
        }
        #endregion

        private int GetUniformLocation(string name)
        {
            if (uniformLocationCache.ContainsKey(name))
                return uniformLocationCache[name];

            int location = glGetUniformLocation(_id, name);
            uniformLocationCache.Add(name, location);

            return location;
        }
    }
}
