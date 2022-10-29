using System;
using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Serialization;
using CrossEngine.Rendering.Culling;

namespace CrossEngine.Rendering.Cameras
{
    public class Camera : ISerializable
    {
        //_viewMatrix = Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation));
        public virtual Matrix4x4 ViewMatrix { get; set; } = Matrix4x4.Identity;
        public virtual Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 ViewProjectionMatrix { get => ViewMatrix * ProjectionMatrix; }
        public Frustum Frustum;

        // TODO: finish this
        private bool _resize = false;
        private float _width, _height;
        private float _near, _far;
        private float _p_fov;
        private float _o_size;

        public Camera()
        {

        }

        public void PrepareFrustum() => Frustum = new Frustum(ProjectionMatrix, ViewMatrix);

        public void SetOrtho()
        {
            throw new NotImplementedException();
        }

        public void SetPerspective()
        {
            throw new NotImplementedException();
        }

        public virtual void Resize(float width, float height) { }

        public virtual void GetObjectData(SerializationInfo info)
        {
            info.AddValue("ProjectionMatrix", ProjectionMatrix);
            info.AddValue("ViewMatrix", ViewMatrix);
        }
        
        public virtual void SetObjectData(SerializationInfo info)
        {
            ProjectionMatrix = info.GetValue("ProjectionMatrix", ProjectionMatrix);
            ViewMatrix = info.GetValue("ViewMatrix", ViewMatrix);
        }
    }
}
