using static OpenGL.GL;

namespace CrossEngine.Rendering.Shaders
{
    public enum ShaderDataType : int
    {
        Float = GL_FLOAT,
        Float2 = GL_FLOAT_VEC2,
        Float3 = GL_FLOAT_VEC3,
        Float4 = GL_FLOAT_VEC4,

        Mat3 = GL_FLOAT_MAT3,
        Mat4 = GL_FLOAT_MAT4,

        Int = GL_INT,
        Int2 = GL_INT_VEC2,
        Int3 = GL_INT_VEC3,
        Int4 = GL_INT_VEC4,

        Bool = GL_BOOL,
    }
}