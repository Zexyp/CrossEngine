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
        public Vector2 ViewportSize;
        public float AspectRatio = 1;

        public abstract void Pan(Vector2 delta);
        public abstract void Move(Vector2 delta);
        public abstract void Zoom(float delta);

        public override void Resize(float width, float height)
        {
            ViewportSize = new Vector2(width, height);
            AspectRatio = ViewportSize.X / ViewportSize .Y;

            MarkProjectionDirty();
        }
    }

    public class OrthographicControllableEditorCamera : ControllableEditorCamera
    {
        private float _orthographicSize = 10.0f;
        private float _zNear = -1.0f;
        private float _zFar = 1.0f;

        [EditorSingleValue]
        public float OrthographicSize
        {
            get => _orthographicSize;
            set
            {
                if (_orthographicSize == value) return;
                _orthographicSize = value;
                MarkProjectionDirty();
            }
        }
        [EditorSingleValue]
        public float ZNear
        {
            get => _zNear;
            set
            {
                if (_zNear == value) return;
                _zNear = value;
                MarkProjectionDirty();
            }
        }
        [EditorSingleValue]
        public float ZFar
        {
            get => _zFar;
            set
            {
                if (_zFar == value) return;
                _zFar = value;
                MarkProjectionDirty();
            }
        }

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

            MarkProjectionDirty();
        }

        public override void Pan(Vector2 delta)
        {
            Move(delta);
        }

        //float pitch = 0.0f, yaw = 0.0f, roll = 0.0f;

        //float cameraTranslationSpeed = 5.0f;

        //public Vector3 GetUpDirection()
        //{
        //    return Vector3.Transform(Vector3.UnitY, Camera.Rotation);
        //}
        //public Vector3 GetRightDirection()
        //{
        //    return Vector3.Transform(Vector3.UnitX, Camera.Rotation);
        //}
        //public Vector3 GetForwardDirection()
        //{
        //    return Vector3.Transform(Vector3.UnitZ, Camera.Rotation);
        //}

        //public Quaternion GetOrientation()
        //{
        //    // mby negate roll
        //    return QuaternionExtension.CreateFromXYZRotation(new Vector3(-pitch, -yaw, roll));
        //}

        //public void OnUpdateX(float timestep)
        //{
        //    Vector2 mouse = new Vector2(Input.GetMouseX(), Input.GetMouseY());
        //    Vector2 delta = (mouse - initialMousePosition) * 0.003f;
        //    initialMousePosition = mouse;
        //
        //    if (Input.IsMouseButtonPressed(Mouse.Middle))
        //        MousePan(delta);
        //
        //    float moveSpeedMultiplier = Input.GetKey(Key.LeftShift) ? 2.0f : (Input.GetKey(Key.LeftControl) ? 0.5f : 1.0f);
        //    moveSpeedMultiplier *= timestep * cameraTranslationSpeed;
        //
        //    if (Input.GetKey(Key.ArrowRight))
        //    {
        //        Camera.Position += moveSpeedMultiplier * GetRightDirection();
        //    }
        //    if (Input.GetKey(Key.ArrowLeft))
        //    {
        //        Camera.Position -= moveSpeedMultiplier * GetRightDirection();
        //    }
        //    
        //    if (Input.GetKey(Key.ArrowUp))
        //    {
        //        Camera.Position += moveSpeedMultiplier * GetUpDirection();
        //    }
        //    if (Input.GetKey(Key.ArrowDown))
        //    {
        //        Camera.Position -= moveSpeedMultiplier * GetUpDirection();
        //    }
        //
        //    cameraTranslationSpeed = zoomLevel;
        //}
        //
        //public void Resize(float width, float height)
        //{
        //    aspectRatio = width / height;
        //    camera.SetProjection(-aspectRatio * zoomLevel, aspectRatio * zoomLevel, -zoomLevel, zoomLevel);
        //}
        //
        //
        //void OnWindowResize(WindowResizeEvent e)
        //{
        //    if (AutoResize)
        //        Resize(e.Width, e.Height);
        //}
    }

    public class PerspectiveControllableEditorCamera : ControllableEditorCamera
    {
        private float _zNear = 0.1f;
        private float _zFar = 100f;
        private float _fov = 60f;
        private float _zoom = 10f;
        Vector2 rotation;

        [EditorSingleValue]
        public float ZNear
        {
            get => _zNear;
            set
            {
                if (_zNear == value) return;
                _zNear = Math.Clamp(value, float.Epsilon, BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(_zFar) - 1));
                MarkProjectionDirty();
            }
        }
        [EditorSingleValue]
        public float ZFar
        {
            get => _zFar;
            set
            {
                if (_zFar == value) return;
                _zFar = Math.Clamp(value, BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(_zNear) + 1), float.MaxValue);
                MarkProjectionDirty();
            }
        }
        [EditorSingleValue]
        public float FOV
        {
            get => _fov;
            set
            {
                if (_fov == value) return;
                _fov = Math.Clamp(value, float.Epsilon, float.MaxValue);
                MarkProjectionDirty();
            }
        }
        [EditorSingleValue]
        public float ZoomDistance
        {
            get => _zoom;
            set
            {
                if (_fov == value) return;
                _zoom = Math.Clamp(value, float.Epsilon, float.MaxValue);
                MarkProjectionDirty();
            }
        }

        protected override Matrix4x4 CreateProjectionMatrix()
        {
            return Matrix4x4.CreateTranslation(new Vector3(0, 0, -_zoom)) * Matrix4x4.CreatePerspectiveFieldOfView(
                MathExtension.ToRadians(_fov),
                AspectRatio,
                _zNear, _zFar);
        }


        // (rotate)
        public override void Move(Vector2 delta)
        {
            if (Math.Abs(rotation.Y) > MathF.PI) delta.X *= -1;
            rotation += delta / 360 * (MathF.PI / 2);
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

            MarkProjectionDirty();
        }
    }
}
