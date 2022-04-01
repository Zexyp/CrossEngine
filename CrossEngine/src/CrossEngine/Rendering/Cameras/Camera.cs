using System.Numerics;

using CrossEngine.Utils.Editor;
//using CrossEngine.Serialization;

namespace CrossEngine.Rendering.Cameras
{
    public class Camera/* : ISerializable*/
    {
        //_viewMatrix = Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation));
        private bool _projectionDirty { get; set; } = true;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
        public virtual Matrix4x4 ViewMatrix { get; set; }
        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (_projectionDirty)
                {
                    _projectionMatrix = CreateProjectionMatrix();
                    _projectionDirty = false;
                }
                return _projectionMatrix;
            }
            set => _projectionMatrix = value;
        }
        public Matrix4x4 ViewProjectionMatrix { get => ViewMatrix * ProjectionMatrix; }

        public Camera()
        {

        }

        protected void MarkProjectionDirty() => _projectionDirty = true;

        public virtual void Resize(float width, float height)
        {
        }

        protected virtual Matrix4x4 CreateProjectionMatrix()
        {
            return _projectionMatrix;
        }

        //#region ISerializable
        //public virtual void OnSerialize(SerializationInfo info)
        //{
        //    info.AddValue("ProjectionMatrix", ProjectionMatrix);
        //}
        //
        //public virtual void OnDeserialize(SerializationInfo info)
        //{
        //    ProjectionMatrix = (Matrix4x4)info.GetValue("ProjectionMatrix", typeof(Matrix4x4));
        //}
        //#endregion
    }
}
