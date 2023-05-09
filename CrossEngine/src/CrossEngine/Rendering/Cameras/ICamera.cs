using CrossEngine.Rendering.Culling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Rendering.Cameras
{
    public interface ICamera
    {
        Matrix4x4 ViewMatrix { get; }
        Matrix4x4 ProjectionMatrix { get; }
        virtual Matrix4x4 ViewProjectionMatrix { get => ViewMatrix * ProjectionMatrix; }

        abstract Frustum Frustum { get; }
    }
}
