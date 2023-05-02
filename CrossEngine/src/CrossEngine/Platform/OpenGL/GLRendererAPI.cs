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
            // TODO: this could be better
            //var window = Application.Instance.Window;
            if (/*window is GlfwWindow*/true)
                Import(Glfw.GetProcAddress);
            else
                Debug.Assert(false, "Window is not supported");
        }

        public override unsafe void DrawIndexed(WeakReference<VertexArray> vertexArray, uint indexCount = 0)
        {
            var vb = vertexArray.GetValue();
            var ib = vb.GetIndexBuffer().GetValue();
            vb.Bind();
            uint count = (indexCount != 0) ? indexCount : ib.Count;
            glDrawElements(GL_TRIANGLES, (int)count, GLUtils.ToGLIndexDataType(ib.DataType), null);
            // TODO: consider unbinding to keep the vertex array state safe
        }

        public override unsafe void DrawArray(WeakReference<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles)
        {
            vertexArray.GetValue().Bind();
            glDrawArrays(GLUtils.ToGLDrawMode(mode), 0, (int)verticesCount);
            // TODO: consider unbinding to keep the vertex array state safe
        }

        public override void Clear() => glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

        public override void SetViewport(uint x, uint y, uint width, uint height) => glViewport((int)x, (int)y, (int)width, (int)height);

        public override void SetClearColor(Vector4 color) => glClearColor(color.X, color.Y, color.Z, color.W);

        public override void SetPolygonMode(PolygonMode mode) => glPolygonMode(GL_FRONT_AND_BACK, GLUtils.ToGLPolygonMode(mode));

        public override void SetDepthFunc(DepthFunc func)
        {
            if (func == DepthFunc.None)
            {
                glDisable(GL_DEPTH_TEST);
                return;
            }

            glEnable(GL_DEPTH_TEST);
            glDepthFunc(GLUtils.ToGLDepthFunc(func));
        }

        public override void SetBlendFunc(BlendFunc func)
        {
            if (func == BlendFunc.None)
            {
                glDisable(GL_BLEND);
                return;
            }

            glEnable(GL_BLEND);
            glBlendFunc(GL_SRC_ALPHA, GLUtils.ToGLBlendFunc(func));
        }

        public override void SetLineWidth(float width) => glLineWidth(width);
    }
}
