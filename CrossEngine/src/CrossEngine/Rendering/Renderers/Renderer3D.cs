using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Logging;
using CrossEngine.Scenes;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Shaders;

namespace CrossEngine.Rendering
{
    public class Renderer3D
    {
        static SceneData sceneData;

        public static void BeginScene(Scene scene, Matrix4x4 viewProjection)
        {
            sceneData = new SceneData(scene, viewProjection);
        }

        public static void EndScene()
        {

        }

        public static void Submit(Shader shader, VertexArray va, Matrix4x4 transformMatrix)
        {
            // uViewProjection cuz MVP
            shader.Use();
            shader.SetMat4("uViewProjection", sceneData.ViewProjectionMatrix);
            shader.SetMat4("uTransform", transformMatrix);

            va.Bind();
            if (va.GetIndexBuffer() != null) Renderer.DrawIndexed(DrawMode.Traingles, va);
            else Renderer.DrawArrays(DrawMode.Traingles, va, int.MaxValue);
        }
    }
}
