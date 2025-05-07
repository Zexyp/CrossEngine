struct PointLight {
    vec3 Position;
    vec3 Color;

    float Radius;
    float Linear;
    float Quadratic;
};

struct SpotLight {
    vec3 Position;
    vec3 Direction;
    vec3 Color;

    float Radius;
    float Linear;
    float Quadratic;
};

struct DirectionalLight {
    vec3 Direction;
    vec3 Color;
};
