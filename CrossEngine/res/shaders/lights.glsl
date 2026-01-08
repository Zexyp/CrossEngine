struct PointLight {
    vec3 Color;
    vec3 Position;

    float Radius;
    float Linear;
    float Quadratic;
};

struct SpotLight {
    vec3 Color;
    vec3 Position;
    vec3 Direction;
    float Angle;
    float Blend;

    float Radius;
    float Linear;
    float Quadratic;
};

struct DirectionalLight {
    vec3 Color;
    vec3 Direction;
};
