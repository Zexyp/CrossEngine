using System;
using System.Numerics;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Utils;
using System.Runtime.InteropServices;


#if WASM
using GLEnum = Silk.NET.OpenGLES.GLEnum;
using static CrossEngine.Platform.Wasm.EGLContext;
#else
using GLEnum = Silk.NET.OpenGL.GLEnum;
using static CrossEngine.Platform.OpenGL.GLContext;
#endif

namespace CrossEngine.Platform.OpenGL
{
    class GLRendererApi : RendererApi
    {
        public override unsafe void Init()
        {
            Log.Debug("opengl info:\nversion: {0}\nrenderer: {1}\nvendor: {2}",
                Marshal.PtrToStringUTF8((IntPtr)gl.GetString(GLEnum.Version)),
                Marshal.PtrToStringUTF8((IntPtr)gl.GetString(GLEnum.Renderer)),
                Marshal.PtrToStringUTF8((IntPtr)gl.GetString(GLEnum.Vendor)));
        }

        public override unsafe void DrawIndexed(WeakReference<VertexArray> vertexArray, uint indexCount = 0)
        {
            var va = vertexArray.GetValue();
            var ib = va.GetIndexBuffer().GetValue();
            va.Bind();
            uint count = (indexCount != 0) ? indexCount : ib.Count;
            gl.DrawElements(GLEnum.Triangles, count, GLUtils.ToGLIndexDataType(ib.DataType), null);
            // TODO: consider unbinding to keep the vertex array state safe
        }

        public override unsafe void DrawArray(WeakReference<VertexArray> vertexArray, uint verticesCount, DrawMode mode = DrawMode.Traingles)
        {
            vertexArray.GetValue().Bind();
            gl.DrawArrays(GLUtils.ToGLDrawMode(mode), 0, verticesCount);
            // TODO: consider unbinding to keep the vertex array state safe
        }

        public override void Clear() => gl.Clear((uint)(GLEnum.ColorBufferBit | GLEnum.DepthBufferBit));

        public override void SetViewport(uint x, uint y, uint width, uint height) => gl.Viewport((int)x, (int)y, width, height);

        public override void SetClearColor(Vector4 color) => gl.ClearColor(color.X, color.Y, color.Z, color.W);

        public override void SetPolygonMode(PolygonMode mode) =>
#if !OPENGL_ES
            gl.PolygonMode(GLEnum.FrontAndBack, GLUtils.ToGLPolygonMode(mode));
#else
            throw new NotSupportedException();
#endif

        public override void SetDepthFunc(DepthFunc func)
        {
            if (func == DepthFunc.None)
            {
                gl.Disable(GLEnum.DepthTest);
                return;
            }

            gl.Enable(GLEnum.DepthTest);
            gl.DepthFunc(GLUtils.ToGLDepthFunc(func));
        }

        public override void SetBlendFunc(BlendFunc func)
        {
            if (func == BlendFunc.None)
            {
                gl.Disable(GLEnum.Blend);
                return;
            }

            gl.Enable(GLEnum.Blend);
            gl.BlendFunc(GLEnum.SrcAlpha, GLUtils.ToGLBlendFunc(func));
        }

        public override void SetLineWidth(float width) => gl.LineWidth(width);
    }
}
