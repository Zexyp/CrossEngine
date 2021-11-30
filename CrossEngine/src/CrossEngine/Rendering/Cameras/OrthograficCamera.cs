using System;

using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngine.Rendering.Cameras
{
    public class OrthographicCamera : Camera
    {
        public OrthographicCamera(float left, float right, float bottom, float top) : base()
        {
            SetProjection(left, right, bottom, top);
        }

        public void SetProjection(float left, float right, float bottom, float top, float near = 0.0f, float far = 2.0f)
        {
            ProjectionMatrix = Matrix4x4Extension.Ortho(left, right, bottom, top, near, far);
        }
    }
}
