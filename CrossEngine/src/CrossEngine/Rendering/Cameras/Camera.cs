using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Rendering.Cameras
{
    public class Camera : ISerializable
    {
        //_viewMatrix = Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation));
        private bool _dirty = true;
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (_dirty)
                {
                    _projectionMatrix = CreateProjectionMatrix();
                    _dirty = false;
                }
                return _projectionMatrix;
            }
            set => _projectionMatrix = value;
        }

        public Camera()
        {

        }

        protected void MarkProjectionDirty() => _dirty = true;

        public virtual void Resize(float width, float height)
        {
        }

        protected virtual Matrix4x4 CreateProjectionMatrix()
        {
            return _projectionMatrix;
        }

        #region ISerializable
        public virtual void OnSerialize(SerializationInfo info)
        {
            info.AddValue("ProjectionMatrix", ProjectionMatrix);
        }

        public virtual void OnDeserialize(SerializationInfo info)
        {
            ProjectionMatrix = (Matrix4x4)info.GetValue("ProjectionMatrix", typeof(Matrix4x4));
        }
        #endregion
    }
}
