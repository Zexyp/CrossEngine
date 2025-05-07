using System;
using System.Numerics;
using System.Threading.Tasks;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Rendering.Materials;
using CrossEngine.Loaders;
using CrossEngine.Assets;
using CrossEngine.Core;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;
using CrossEngine.Components;

namespace Scripting;

public class ScriptMaterial : IMaterial
{
    private const string ShaderSource = @"
#type vertex
#version 330 core
#include ""internal:layout_in_generator.glsl""

uniform mat4 uViewProjection = mat4(1);
uniform mat4 uModel = mat4(1);

out vec3 vPosition;
out vec3 vNormal;
out vec2 vTexCoord;

void main() {
    vPosition = (uModel * vec4(aPosition, 1.0)).xyz;
    vNormal = mat3(transpose(inverse(uModel))) * aNormal;
    vTexCoord = aTexCoord;

	gl_Position = uViewProjection * uModel * vec4(aPosition, 1.0);
}

#type fragment
#version 330 core
#include ""internal:layout_out_deferred.glsl""

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord;

uniform int uEntityID;
uniform vec3 uColor;
uniform sampler2D uTexture;

void main() {
	oColor = vec4(uColor * texture(uTexture, vTexCoord).xyz, 128);
    oEntityID = uEntityID;
    oPosition = vPosition;
    oNormal = vNormal;
}
";
    
    public WeakReference<ShaderProgram> Shader { get; set; }

    public void Update(ShaderProgram shader)
    {
        shader.Use();
        shader.SetParameterVec3("uColor", ColorHelper.HSVToRGB(new Vector3(Time.ElapsedF, .5f, 1)));
        TextureLoader.DefaultTexture.GetValue().Bind();
    }

    public ScriptMaterial()
    {
        Shader = ShaderPreprocessor.CreateProgramFromString(ShaderSource);
    }
}

public class Script : Behaviour
{
    public override void Start()
    {
        Component.Entity.GetComponent<MeshRendererComponent>().Material =
            new MaterialAsset() { Material = new ScriptMaterial() };
    }
}
