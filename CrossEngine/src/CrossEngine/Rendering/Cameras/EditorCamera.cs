using System.Numerics;

using CrossEngine.Utils.Editor;

namespace CrossEngine.Rendering.Cameras
{
    public class EditorCamera : Camera
    {
        // TODO: seri
        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private bool _viewDirty = true;

        [EditorVector3Value]
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    _viewDirty = true;
                }
            }
        }
        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    _viewDirty = true;
                }
            }
        }

        public override Matrix4x4 ViewMatrix
        {
            get
            {
                if (_viewDirty)
                {
                    _viewMatrix = Matrix4x4.CreateTranslation(-Position) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Rotation));
                    _viewDirty = false;
                }
                return _viewMatrix;
            }
        }
    }
}
