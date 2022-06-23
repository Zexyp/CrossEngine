using System;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
//using CrossEngine.Serialization;

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
        [EditorSingleValue]
        public float FOV
        {
            get => _fov;
            set
            {
                if (_fov == value) return;
                _fov = value;
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
                if (_aspectRatio <= 0) _aspectRatio = 1;
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
            return Matrix4x4Extension.Perspective(MathExtension.ToRadians(_fov), AspectRatio, _zNear, _zFar);
        }

        //#region Serialization
        //public override void OnSerialize(SerializationInfo info)
        //{
        //    base.OnSerialize(info);
        //
        //    info.AddValue("FOV", _fov);
        //    info.AddValue("ZNear", _zNear);
        //    info.AddValue("ZFar", _zFar);
        //    info.AddValue("AspectRatio", _aspectRatio);
        //}
        //
        //public override void OnDeserialize(SerializationInfo info)
        //{
        //    base.OnDeserialize(info);
        //
        //    _fov = info.GetValue<float>("FOV");
        //    _zNear = info.GetValue<float>("ZNear");
        //    _zFar = info.GetValue<float>("ZFar");
        //    _aspectRatio = info.GetValue<float>("AspectRatio");
        //    MarkProjectionDirty();
        //}
        //#endregion
    }
}
