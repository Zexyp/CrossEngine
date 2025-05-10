struct WavefrontMaterial {
    vec3 Ambient;
    vec3 Diffuse;
    vec3 Specular;
    vec3 Emissive;
    float SpecularExponent;
    float Disolve;
    float RefractiveIndex;
    sampler2D MapDiffuse;
    sampler2D MapSpecular;
    sampler2D MapSpecularHighlight;
    sampler2D MapNormal;
};
