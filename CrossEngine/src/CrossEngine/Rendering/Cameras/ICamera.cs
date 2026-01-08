using CrossEngine.Rendering.Culling;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Geometry;

namespace CrossEngine.Rendering.Cameras
{
    public interface ICamera
    {
        Matrix4x4 ProjectionMatrix { get; }
        virtual Matrix4x4 GetViewProjectionMatrix() => GetViewMatrix() * ProjectionMatrix;
        Matrix4x4 GetViewMatrix();

        virtual Frustum GetFrustum() => Frustum.Create(ProjectionMatrix, GetViewMatrix());
    }

    public interface IResizableCamera : ICamera
    {
        void Resize(float width, float height);
    }
}
