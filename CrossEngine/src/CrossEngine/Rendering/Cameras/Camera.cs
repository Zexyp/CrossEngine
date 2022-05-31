using System.Numerics;

using CrossEngine.Utils.Editor;
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

        public Camera()
        {

        }

        public void PrepareFrustum() => Frustum = new Frustum(ProjectionMatrix, ViewMatrix);

        public virtual void Resize(float width, float height) { }

        public virtual void GetObjectData(SerializationInfo info)
        {
            info.AddValue("ProjectionMatrix", ProjectionMatrix);
            info.AddValue("ViewMatrix", ViewMatrix);
        }
        
        public virtual void SetObjectData(SerializationInfo info)
        {
            ProjectionMatrix = (Matrix4x4)info.GetValue("ProjectionMatrix", typeof(Matrix4x4));
            ViewMatrix = (Matrix4x4)info.GetValue("ViewMatrix", typeof(Matrix4x4));
        }
    }
}
