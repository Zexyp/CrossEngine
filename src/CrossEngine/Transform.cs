using System;

using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngine
{
    public class Transform
    {
        private Vector3 _position = Vector3.Zero;
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _scale = Vector3.One;

        public Vector3 Position { 
            get { return _position; }
            set { _position = value; Update(); } }
        public Quaternion Rotation { 
            get { return _rotation; }
            set { _rotation = value; Update(); } }
        public Vector3 Scale { 
            get { return _scale; }
            set { _scale = value; Update(); } }

        public Matrix4x4 TransformMatrix { get; private set; } // = Matrix4x4.Identity;

        public event EventHandler<EventArgs> OnValueChanged;

        public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this._position = position;
            this._rotation = rotation;
            this._scale = scale;
            Update();
        }
        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            this._position = position;
            this._rotation = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            this._scale = scale;
            Update();
        }
        public Transform(Vector3 position, Quaternion rotation)
        {
            this._position = position;
            this._rotation = rotation;
            Update();
        }
        public Transform(Vector3 position, Vector3 rotation)
        {
            this._position = position;
            this._rotation = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
            Update();
        }
        public Transform(Vector3 position)
        {
            this._position = position;
            Update();
        }
        public Transform()
        {
            Update();
        }

        private void Update()
        {
            TransformMatrix = GetTransformMatrix();
            OnValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public override string ToString()
        {
            return "pos: " + _position.ToString() + "; rot: " + _rotation.ToString() + "; scl: " + _scale.ToString();
        }

        #region Getters
        private Matrix4x4 GetTransformMatrix()
        {
            Matrix4x4 translationMat = Matrix4x4.CreateTranslation(_position);
            //Matrix4x4 rotationMat = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
            Matrix4x4 rotationMat = Matrix4x4.CreateFromQuaternion(_rotation);
            Matrix4x4 scaleMat = Matrix4x4.CreateScale(_scale);
            return scaleMat * rotationMat * translationMat;
        }

        public Matrix4x4 GetTranslationMatrix()
        {
            return Matrix4x4.CreateTranslation(_position);
        }

        public Matrix4x4 GetRotationMatrix()
        {
            //Matrix4x4 XrotationMat = Matrix4x4.CreateRotationX(rotation.X);
            //Matrix4x4 YrotationMat = Matrix4x4.CreateRotationY(rotation.Y);
            //Matrix4x4 ZrotationMat = Matrix4x4.CreateRotationZ(rotation.Z);
            //return ZrotationMat * YrotationMat * XrotationMat;
            return Matrix4x4.CreateFromQuaternion(_rotation);
        }

        public Matrix4x4 GetScaleMatrix()
        {
            return Matrix4x4.CreateScale(_scale);
        }
        #endregion

        #region Setters
        public void SetFromMatrix(Matrix4x4 matrix)
        {
            _position = matrix.Translation;
            _rotation = Quaternion.CreateFromRotationMatrix(matrix);
            //_scale = new Vector3(matrix.M11, matrix.M22, matrix.M33); // this is wrong when obtaining rotation
            Update();
        }
        #endregion
    }
}
