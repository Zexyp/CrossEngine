using System;
using static OpenGL.GL;

using System.Numerics;

using CrossEngine.Rendering.Shading;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering
{
    class MeshRenderer
    {
        static public unsafe void DrawMesh(Mesh mesh, Camera camera, Transform transform, Material material)
        {
            material.shader.Use();

            material.Bind(); // binds custom variables

            material.shader.SetMat4("projection", camera.ProjectionMatrix);
            material.shader.SetMat4("view", camera.ViewMatrix);
            material.shader.SetMat4("model", transform.TransformMatrix);

            mesh.va.Bind();
            //mesh.ib.Bind();

            glDrawElements(GL_TRIANGLES, mesh.ib.GetCount(), GL_UNSIGNED_INT, (void*)0);
        }
    }
}
