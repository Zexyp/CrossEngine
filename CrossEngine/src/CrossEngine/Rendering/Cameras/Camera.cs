using System;
using System.Numerics;
using CrossEngine.Geometry;
using CrossEngine.Rendering.Culling;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Cameras
{
    public class Camera : ICamera
    {
        //_viewMatrix = Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation));
        public Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.CreateScale(0.1f);

        public Frustum Frustum => Frustum.Create(ProjectionMatrix, ViewMatrix);

        public void SetOrtho(float width, float height, float near = 1, float far = -1)
        {
            ProjectionMatrix = Matrix4x4Extension.CreateOrthographic(width, height, near, far);
        }

        public void SetPerspective(float fov, float aspect, float near = .1f, float far = 1000)
        {
            ProjectionMatrix = Matrix4x4Extension.CreatePerspectiveFieldOfView(fov, aspect, near, far);
        }

        public Matrix4x4 GetViewMatrix() => ViewMatrix;

        //public virtual void GetObjectData(SerializationInfo info)
        //{
        //    info.AddValue("ProjectionMatrix", ProjectionMatrix);
        //    info.AddValue("ViewMatrix", ViewMatrix);
        //}
        //
        //public virtual void SetObjectData(SerializationInfo info)
        //{
        //    ProjectionMatrix = (Matrix4x4)info.GetValue("ProjectionMatrix", typeof(Matrix4x4));
        //    ViewMatrix = (Matrix4x4)info.GetValue("ViewMatrix", typeof(Matrix4x4));
        //}
    }
}
