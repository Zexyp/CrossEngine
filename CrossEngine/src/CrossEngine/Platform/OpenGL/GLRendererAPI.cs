using System;
using static OpenGL.GL;
using GLFW;

using System.Diagnostics;
using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using CrossEngine.Platform.Windows;

namespace CrossEngine.Platform.OpenGL
{
    class GLRendererAPI : RendererAPI
    {
        public override void Init()
        {
            var window = Application.Instance.GetWindow();
            if (window is GLFWWindow)
                Import(Glfw.GetProcAddress);
            else
                Debug.Assert(false, "Window is not supported");
        }

        public override unsafe void DrawIndexed(Ref<VertexArray> vertexArray, uint indexCount = 0)
        {
            var ib = vertexArray.Value.GetIndexBuffer();
            ((VertexArray)vertexArray).Bind();
            uint count = (indexCount != 0) ? indexCount : ((IndexBuffer)ib).Count;
            glDrawElements(GL_TRIANGLES, (int)count, GLUtils.ToGLIndexDataType(((IndexBuffer)ib).DataType), null);
            // TODO: consider unbinding to keep the vertex array state safe
        }

        public override unsafe void DrawArray(Ref<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles)
        {
            ((VertexArray)vertexArray).Bind();
            glDrawArrays(GLUtils.ToGLDrawMode(mode), 0, (int)verticesCount);
            // TODO: consider unbinding to keep the vertex array state safe
        }

        public override void Clear() => glClear(GL_COLOR_BUFFER_BIT);

        public override void SetViewport(uint x, uint y, uint width, uint height) => glViewport((int)x, (int)y, (int)width, (int)height);

        public override void SetClearColor(Vector4 color) => glClearColor(color.X, color.Y, color.Z, color.W);

        public override void SetPolygonMode(PolygonMode mode) => glPolygonMode(GL_FRONT_AND_BACK, GLUtils.ToGLPolygonMode(mode));

        public override void SetDepthFunc(DepthFunc func) => throw new NotImplementedException();
    }
}
