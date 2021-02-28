using System;
using static OpenGL.GL;

using System.Numerics;

using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Utils;
using CrossEngine.Rendering.Shading;
using CrossEngine.Rendering.Cameras;

namespace CrossEngine.Rendering
{
    public class Skybox
    {
        static VertexBuffer vb;
        static IndexBuffer ib;
        static VertexArray va;

        public Texture texture;

        Shader shader;

        public Skybox(string cubemapPath)
        {
            if(va == null && ib == null && vb == null)
                SetupSkyboxGeometry();

            texture = AssetManager.Textures.GetCubeMap(cubemapPath);
            shader = AssetManager.Shaders.GetShader("shaders/skybox/vertskybox.shader", "shaders/skybox/fragskybox.shader");
        }

        public void Dispose()
        {
            va.Dispose();
            vb.Dispose();
            ib.Dispose();

            texture.Dispose();
        }

        unsafe void SetupSkyboxGeometry()
        {
            //new Vector3(-1.0f, -1.0f, -1.0f),a
            //new Vector3(+1.0f, -1.0f, -1.0f),b
            //new Vector3(-1.0f, +1.0f, -1.0f),c
            //new Vector3(+1.0f, +1.0f, -1.0f),d
            //new Vector3(-1.0f, -1.0f, +1.0f),e
            //new Vector3(+1.0f, -1.0f, +1.0f),f
            //new Vector3(-1.0f, +1.0f, +1.0f),g
            //new Vector3(+1.0f, +1.0f, +1.0f),h

            Vector3[] skyboxVertices = new Vector3[] {
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(+1.0f, -1.0f, -1.0f),
                new Vector3(+1.0f, +1.0f, -1.0f),
                new Vector3(-1.0f, +1.0f, -1.0f),

                new Vector3(+1.0f, -1.0f, +1.0f),
                new Vector3(-1.0f, -1.0f, +1.0f),
                new Vector3(-1.0f, +1.0f, +1.0f),
                new Vector3(+1.0f, +1.0f, +1.0f),

                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, +1.0f),
                new Vector3(+1.0f, -1.0f, +1.0f),
                new Vector3(+1.0f, -1.0f, -1.0f),

                new Vector3(-1.0f, +1.0f, -1.0f),
                new Vector3(+1.0f, +1.0f, -1.0f),
                new Vector3(+1.0f, +1.0f, +1.0f),
                new Vector3(-1.0f, +1.0f, +1.0f),

                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(-1.0f, +1.0f, -1.0f),
                new Vector3(-1.0f, +1.0f, +1.0f),
                new Vector3(-1.0f, -1.0f, +1.0f),

                new Vector3(+1.0f, +1.0f, -1.0f),
                new Vector3(+1.0f, -1.0f, -1.0f),
                new Vector3(+1.0f, -1.0f, +1.0f),
                new Vector3(+1.0f, +1.0f, +1.0f)
            };

            uint offset = 0;
            uint[] indices = new uint[36];
            for (uint i = 0; i < 36; i += 6)
            {
                indices[i + 0] = offset + 0;
                indices[i + 1] = offset + 1;
                indices[i + 2] = offset + 2;

                indices[i + 3] = offset + 2;
                indices[i + 4] = offset + 3;
                indices[i + 5] = offset + 0;
                offset += 4;
            }

            va = new VertexArray();
            va.Bind();
            fixed (Vector3* verteciesp = &skyboxVertices[0])
                vb = new VertexBuffer(verteciesp, sizeof(Vector3) * skyboxVertices.Length);
            fixed (uint* indicesp = &indices[0])
                ib = new IndexBuffer(indicesp, indices.Length);

            VertexBufferLayout layout = new VertexBufferLayout();
            layout.RawAdd(VertexBufferElementType.Float, 3);

            va.AddBuffer(vb, layout);
        }

        public unsafe void Draw(Camera camera)
        {
            glDepthFunc(GL_LEQUAL);

            shader.Use();
            shader.SetMat4("projection", camera.ProjectionMatrix);
            shader.SetMat4("view", Matrix4x4Extension.ClearTranslation(camera.ViewMatrix));

            texture.BindTo(0);

            va.Bind();
            //ib.Bind();

            glDrawElements(GL_TRIANGLES, ib.GetCount(), GL_UNSIGNED_INT, (void*)0);

            glDepthFunc(GL_LESS);
        }
    }
}
