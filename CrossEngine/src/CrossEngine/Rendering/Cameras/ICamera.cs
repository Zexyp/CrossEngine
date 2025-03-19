using CrossEngine.Rendering.Culling;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Rendering.Cameras
{
    public interface ICamera : ITransform
    {
        Matrix4x4 ProjectionMatrix { get; set; }
        virtual Matrix4x4 GetViewProjectionMatrix() => GetViewMatrix() * ProjectionMatrix;
        virtual Matrix4x4 GetViewMatrix() => Matrix4x4Extension.Invert(GetMatrix());

        virtual Frustum Frustum { get => throw new NotImplementedException(); }
    }
}
