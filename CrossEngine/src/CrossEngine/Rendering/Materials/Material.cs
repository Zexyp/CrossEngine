using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using CrossEngine.Loaders;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;

namespace CrossEngine.Rendering.Materials;

public interface IMaterial
{
    ShaderProgram Shader { get; set; }
    void Update(ShaderProgram shader);
}

public class DynamicMaterial : IMaterial
{
    public ShaderProgram Shader { get; set; }
    public readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();
    public readonly Dictionary<string, Texture> Samplers = new Dictionary<string, Texture>();

    public DynamicMaterial(ShaderProgram shader = null)
    {
        Shader = shader;
    }

    public void Use()
    {
        var shader = Shader;
        shader.Use();
        
        Update(shader);
    }
    
    public void Update(ShaderProgram shader)
    {
        UpdateParameters(shader);
        UpdateSamplers(shader);
    }

    private void UpdateParameters(ShaderProgram shader)
    {
        foreach (var pair in Parameters)
        {
            switch (pair.Value)
            {
                case int value: shader.SetParameterInt(pair.Key, value); break;
                case float value: shader.SetParameterFloat(pair.Key, value); break;
                case Vector2 value: shader.SetParameterVec2(pair.Key, value); break;
                case Vector3 value: shader.SetParameterVec3(pair.Key, value); break;
                case Vector4 value: shader.SetParameterVec4(pair.Key, value); break;
                case Matrix4x4 value: shader.SetParameterMat4(pair.Key, value); break;
                default: Debug.Assert(false); break;
            }
        }
    }

    private void UpdateSamplers(ShaderProgram shader)
    {
        int slot = 0;
        foreach (var pair in Samplers)
        {
            shader.SetParameterInt(pair.Key, slot);
            pair.Value.Bind((uint)slot);
            slot++;
        }
    }
}

class DefaultMaterial : IMaterial
{
    private const string DefaultShaderSource =
@"#type vertex
#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
uniform mat4 uViewProjection = mat4(1);
uniform mat4 uModel = mat4(1);
void main() {
    vTexCoord = aTexCoord;
    gl_Position = uViewProjection * uModel * vec4(aPosition, 1.0);
}

#type fragment
#version 330 core
layout(location = 0) out vec4 oColor;
layout(location = 1) out int oEntityIDColor;
in vec2 vTexCoord;
uniform int uEntityID;
void main() {
    oColor = vec4(1, 0, 1, 1);
    oEntityIDColor = uEntityID;
}";

    static ShaderProgram _shader = ShaderPreprocessor.CreateProgramFromString(DefaultShaderSource);

    public ShaderProgram Shader { get => _shader; set => throw new InvalidOperationException(); }

    public void Update(ShaderProgram shader)
    {
    }
}
