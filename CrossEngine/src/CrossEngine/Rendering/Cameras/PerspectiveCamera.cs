﻿using System;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Rendering.Cameras
{
    public class PerspectiveCamera : Camera
    {
        private float _zNear = 0.1f;
        private float _zFar = 100f;
        private float _fov = 60f;
        private float _aspectRatio = 1.0f;

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
        public float AspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (_aspectRatio == value) return;
                _aspectRatio = value;
                MarkProjectionDirty();
            }
        }

        public PerspectiveCamera()
        {
            
        }

        public override void Resize(float width, float height)
        {
            AspectRatio = width / height;
        }

        protected override Matrix4x4 CreateProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathExtension.ToRadians(Math.Clamp(_fov, 0, 179.999985f)), _aspectRatio, _zNear, _zFar);
        }

        #region Serialization
        public override void OnSerialize(SerializationInfo info)
        {
            base.OnSerialize(info);

            info.AddValue("FOV", _fov);
            info.AddValue("ZNear", _zNear);
            info.AddValue("ZFar", _zFar);
            info.AddValue("AspectRatio", _aspectRatio);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            base.OnDeserialize(info);

            _fov = info.GetValue<float>("FOV");
            _zNear = info.GetValue<float>("ZNear");
            _zFar = info.GetValue<float>("ZFar");
            _aspectRatio = info.GetValue<float>("AspectRatio");
            MarkProjectionDirty();
        }
        #endregion
    }
}
