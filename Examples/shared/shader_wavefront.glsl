#type vertex
#version 330 core
#include "internal:layout_in_wavefront.glsl"

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
#include "internal:layout_out_deferred.glsl"
#include "internal:material_wavefront.glsl"

in vec3 vPosition;
in vec3 vNormal;
in vec2 vTexCoord;

uniform WavefrontMaterial uMaterial;
uniform int uEntityID;

void main() {
	oColor = vec4(uMaterial.Diffuse * texture(uMaterial.MapDiffuse, vTexCoord).xyz, uMaterial.SpecularExponent);
    oEntityID = uEntityID;
    oPosition = vPosition;
    oNormal = vNormal;
}
