using System;
using static OpenGL.GL;

using System.Numerics;

using CrossEngine.Rendering.Errors;

namespace CrossEngine.Rendering
{
    public static class Renderer
    {
        static public void Clear()
        {
            glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        }

        static public void ClearDepth()
        {
            glClear(GL_DEPTH_BUFFER_BIT);
        }

        static public void ClearColor()
        {
            glClear(GL_COLOR_BUFFER_BIT);
        }

        static public void SetClearColor(Vector3 color)
        {
            glClearColor(color.X, color.Y, color.Z, 1.0f);
        }

        static public void SetClearColor(float r = 0.0f, float g = 0.0f, float b = 0.0f, float a = 0.0f)
        {
            glClearColor(r, g, b, a);
        }

        static public void SetViewport(int x, int y, int width, int height)
        {
            glViewport(x, y, width, height);
        }

        public static void EnableBlending(bool enable)
        {
            if (enable)
                glEnable(GL_BLEND);
            else
                glDisable(GL_BLEND);

            glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
        }

        public static void EnableDepthTest(bool enable)
        {
            if (enable)
                glEnable(GL_DEPTH_TEST);
            else
                glDisable(GL_DEPTH_TEST);
        }

        public static void SetPolygonMode(PolygonMode polygonMode)
        {
            glPolygonMode(GL_FRONT_AND_BACK, (int)polygonMode);
        }
    }

    public enum PolygonMode
    {
        Fill = GL_FILL,
        Line = GL_LINE,
        Point = GL_POINT
    }
}
