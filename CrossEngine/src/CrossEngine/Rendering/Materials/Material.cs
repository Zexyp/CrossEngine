using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Textures;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Materials;

public class Material
{
    public WeakReference<ShaderProgram> Shader;
    public Dictionary<string, object> Parameters = new Dictionary<string, object>();
    public Dictionary<string, WeakReference<Texture>> Samplers = new Dictionary<string, WeakReference<Texture>>();

    public Material(WeakReference<ShaderProgram> shader = null)
    {
        Shader = shader;
    }

    public void Use()
    {
        var shader = Shader.GetValue();
        shader.Use();
        
        Update(shader);
    }
    
    void Update(ShaderProgram shader)
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
            pair.Value.GetValue().Bind((uint)slot);
            slot++;
        }
    }
}