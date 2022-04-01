﻿using System;

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

        public override void Resize(float width, float height)
        {
            ViewportSize = new Vector2(width, height);
            AspectRatio = ViewportSize.X / ViewportSize.Y;

            MarkProjectionDirty();
        }
    }

    public class OrthographicControllableEditorCamera : ControllableEditorCamera
    {
        private float _orthographicSize = 10.0f;
        private float _zNear = -1.0f;
        private float _zFar = 1.0f;

        [EditorDrag]
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
        [EditorDrag]
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
        [EditorDrag]
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
    }

    public class PerspectiveControllableEditorCamera : ControllableEditorCamera
    {
        private float _zNear = 0.1f;
        private float _zFar = 100f;
        private float _fov = 60f;
        private float _zoom = 10f;
        Vector2 rotation;

        [EditorDrag]
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
        [EditorDrag]
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
        [EditorDrag]
        public float FOV
        {
            get => _fov;
            set
            {
                if (_fov == value) return;
                _fov = Math.Clamp(value, float.Epsilon, 179.999985f);
                MarkProjectionDirty();
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
                MarkProjectionDirty();
            }
        }

        protected override Matrix4x4 CreateProjectionMatrix()
        {
            return Matrix4x4.CreateTranslation(new Vector3(0, 0, _zoom)) * Matrix4x4Extension.Perspective(MathExtension.ToRadians(_fov), AspectRatio, _zNear, _zFar);
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

            MarkProjectionDirty();
        }
    }
}
