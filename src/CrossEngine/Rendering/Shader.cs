using System;
using static OpenGL.GL;

using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CrossEngine.Rendering.Shading
{
    public class Shader
    {
        public uint id = 0;

        Dictionary<string, int> uniformLocationCache = new Dictionary<string, int> { }; // for optimalization

        /*
        public Shader(string vertexPath, string fragmentPath)
        {
            string vertexCode = " ";
            string fragmentCode = " ";

            try
            {
                FileStream vertexShaderFile = new FileStream(vertexPath, FileMode.Open, FileAccess.Read);
                FileStream fragmentShaderFile = new FileStream(fragmentPath, FileMode.Open, FileAccess.Read);

                StreamReader streamReader;

                streamReader = new StreamReader(vertexShaderFile);
                vertexCode = streamReader.ReadToEnd();

                streamReader.Close();

                streamReader = new StreamReader(fragmentShaderFile);
                fragmentCode = streamReader.ReadToEnd();

                streamReader.Close();

                vertexShaderFile.Close();
                fragmentShaderFile.Close();
            }
            catch (FileNotFoundException)
            {
                Log.Error("shader source not found!");
            }
            catch (DirectoryNotFoundException)
            {
                Log.Error("shader source not found!");
            }
            catch (ArgumentException)
            {
                Log.Error("shader creation failed horribly!");
            }

            CreateProgram(vertexCode, fragmentCode);
        }

        public Shader(string path)
        {
            string source = " ";

            try
            {
                FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);

                StreamReader streamReader;

                streamReader = new StreamReader(file);
                source = streamReader.ReadToEnd();

                streamReader.Close();

                file.Close();
            }
            catch (FileNotFoundException)
            {
                Log.Error("shader source not found!");
            }
            catch (DirectoryNotFoundException)
            {
                Log.Error("shader source not found!");
            }
            catch (ArgumentException)
            {
                Log.Error("shader creation failed horribly!");
            }

            string[] result = Regex.Split(source, @"#type+[ ]+([a-zA-Z]+)"); // I hate the regex syntax

            //Log.Debug("elements: " + result.Length);
            //for (int i = 0; i < result.Length; i++)
            //{
            //    Log.Debug(result[i]);
            //}

            if (result.Length != 5)
            {
                Log.Error("unexpected shader format");
                return;
            }

            if(result[1] == "vertex" && result[3] == "fragment")
            {
                CreateProgram(result[2], result[4]);
            }
            else if (result[1] == "fragment" && result[3] == "vertex")
            {
                CreateProgram(result[4], result[2]);
            }
            else
            {
                Log.Error("unexpected shader format");
            }
        }
        */
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
            id = glCreateProgram();
            glAttachShader(id, vertex);
            glAttachShader(id, fragment);

            glLinkProgram(id);

            CheckLinkingErrors(id);

            // cleanup
            glDetachShader(id, vertex);
            glDetachShader(id, fragment);

            glDeleteShader(vertex);
            glDeleteShader(fragment);
        }

        // cleanup
        public void Dispose()
        {
            glDeleteProgram(id);
        }

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
                Log.Error("shader compilation failed!\n" + glGetShaderInfoLog(shader, length));

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
                Log.Error("shader linking failed!\n" + glGetProgramInfoLog(program, length));

                glDeleteProgram(program);

                // to reset the id
                id = 0;
                
                return true;
            }
            return false;
        }

        public void Use()
        {
            glUseProgram(id);
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

            int location = glGetUniformLocation(id, name);
            uniformLocationCache.Add(name, location);

            return location;
        }
    }
}
