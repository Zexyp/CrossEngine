using System;

using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Inputs;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering.Cameras;

namespace CrossEngineEditor
{
    abstract public class ControllableEditorCamera : EditorCamera
    {
        // in pixels; needed to resolve input amount
        public Vector2 ViewportSize;
        private float _aspectRatio;
        public float AspectRatio
        {
            get { return _aspectRatio; }
            set
            {
                _aspectRatio = value;
                if (_aspectRatio <= 0) _aspectRatio = 1;
            }
        }

        public abstract void Pan(Vector2 delta);
        public abstract void Move(Vector2 delta);
        public abstract void Zoom(float delta);
        public abstract void Fly(Vector3 delta, Vector2 mouse);
        protected abstract Matrix4x4 CreateProjectionMatrix();

        public override void Resize(float width, float height)
        {
            ViewportSize = new Vector2(width, height);
            AspectRatio = ViewportSize.X / ViewportSize.Y;

            ProjectionMatrix = CreateProjectionMatrix();
        }
    }

    public class OrthographicControllableEditorCamera : ControllableEditorCamera
    {
        private float _orthographicSize = 10.0f;
        private float _zNear = -1.0f;
        private float _zFar = 1.0f;

        #region Properies
        [EditorDrag]
        public float OrthographicSize
        {
            get => _orthographicSize;
            set
            {
                if (_orthographicSize == value) return;
                _orthographicSize = value;
                ProjectionMatrix = CreateProjectionMatrix();
            }
        }
        [EditorDrag]
        public float ZNear
        {
            get => _zNear;
            set
            {
                if (_zNear == value) return;
                _zNear = value;
                ProjectionMatrix = CreateProjectionMatrix();
            }
        }
        [EditorDrag]
        public float ZFar
        {
            get => _zFar;
            set
            {
                if (_zFar == value) return;
                _zFar = value;
                ProjectionMatrix = CreateProjectionMatrix();
            }
        }
        #endregion

        protected override Matrix4x4 CreateProjectionMatrix()
        {
            return Matrix4x4Extension.Ortho(-_orthographicSize * AspectRatio, _orthographicSize * AspectRatio, -_orthographicSize, _orthographicSize, ZNear, ZFar);
        }



        public override void Move(Vector2 delta)
        {
            Vector2 move = new Vector2(delta.X / ViewportSize.Y, delta.Y / ViewportSize.Y) * OrthographicSize * 2;
            Position += new Vector3(move, 0.0f);
        }

        public override void Zoom(float delta)
        {
            OrthographicSize -= delta * 0.25f / (1 / OrthographicSize * 4);
            OrthographicSize = Math.Max(OrthographicSize, 0.1f);

            ProjectionMatrix = CreateProjectionMatrix();
        }

        public override void Pan(Vector2 delta)
        {
            Move(delta);
        }

        public override void Fly(Vector3 delta, Vector2 mouse)
        {
            Position += new Vector3(delta.X, delta.Z, delta.Y) * OrthographicSize;
            Zoom(mouse.X * 0.05f);
        }
    }

    public class PerspectiveControllableEditorCamera : ControllableEditorCamera
    {
        private float _zNear = 0.1f;
        private float _zFar = 100f;
        private float _fov = 60f;
        private float _zoom = 10f;
        Vector2 rotation;

        public override Matrix4x4 ViewMatrix => base.ViewMatrix * Matrix4x4.CreateTranslation(new Vector3(0, 0, _zoom));

        #region Properies
        [EditorDrag]
        public float ZNear
        {
            get => _zNear;
            set
            {
                if (_zNear == value) return;
                _zNear = Math.Clamp(value, float.Epsilon, BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(_zFar) - 1));
                ProjectionMatrix = CreateProjectionMatrix();
            }
        }
        [EditorDrag]
        public float ZFar
        {
            get => _zFar;
            set
            {
                if (_zFar == value) return;
                _zFar = Math.Clamp(value, BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(_zNear) + 1), float.MaxValue);
                ProjectionMatrix = CreateProjectionMatrix();
            }
        }
        [EditorDrag(Min = 1, Max = 179.999985f)]
        public float FOV
        {
            get => _fov;
            set
            {
                if (_fov == value) return;
                _fov = value;
                ProjectionMatrix = CreateProjectionMatrix();
            }
        }
        [EditorDrag]
        public float ZoomDistance
        {
            get => _zoom;
            set
            {
                if (_fov == value) return;
                _zoom = Math.Clamp(value, float.Epsilon, float.MaxValue);
                ProjectionMatrix = CreateProjectionMatrix();
            }
        }
        #endregion

        protected override Matrix4x4 CreateProjectionMatrix()
        {
            return Matrix4x4Extension.Perspective(MathExtension.ToRadians(_fov), AspectRatio, _zNear, _zFar);
        }


        // (rotate)
        public override void Move(Vector2 delta)
        {
            // input direction correction
            if (Math.Abs(rotation.Y) + MathF.PI / 2 > MathF.PI) delta.X *= -1;

            //                  magical number
            rotation -= delta / 360 * (MathF.PI / 2);
            rotation = new Vector2(rotation.X % (2*MathF.PI), rotation.Y % (2*MathF.PI));

            Rotation = Quaternion.CreateFromYawPitchRoll(
                ( rotation.X),
                (-rotation.Y), 0);
        }

        public override void Pan(Vector2 delta)
        {
            Vector2 move = new Vector2(delta.X / ViewportSize.Y, delta.Y / ViewportSize.Y) * _zoom;
            Position += Vector3.Transform(new Vector3(move, 0.0f), Rotation);
        }

        public override void Zoom(float delta)
        {
            ZoomDistance -= delta * 0.25f / (1 / _zoom * 4);

            ProjectionMatrix = CreateProjectionMatrix();
        }

        public override void Fly(Vector3 delta, Vector2 mouse)
        {
            Vector3 rotated = Vector3.Transform(delta, Rotation);
            Position += rotated * _zoom;
            Position -= Vector3.Transform(Vector3.UnitZ, Rotation) * _zoom;

            Move(-mouse * new Vector2(1, -1) / 2);

            Position += Vector3.Transform(Vector3.UnitZ, Rotation) * _zoom;
        }
    }
}
