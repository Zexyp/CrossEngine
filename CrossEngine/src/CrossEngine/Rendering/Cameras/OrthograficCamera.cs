using System;

using System.Numerics;

using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
//using CrossEngine.Serialization;

namespace CrossEngine.Rendering.Cameras
{
    public class OrthographicCamera : Camera
    {
        private float _orthographicSize = 1.0f;
        private float _zNear = -1.0f;
        private float _zFar = 1.0f;
        private float _aspectRatio = 1.0f;

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

        public OrthographicCamera()
        {

        }

        public override void Resize(float width, float height)
        {
            AspectRatio = width / height;
        }

        protected override Matrix4x4 CreateProjectionMatrix()
        {
            return Matrix4x4Extension.Ortho(-_orthographicSize * _aspectRatio, _orthographicSize * _aspectRatio, -_orthographicSize, _orthographicSize, ZNear, ZFar);
        }

        //#region Serialization
        //public override void OnSerialize(SerializationInfo info)
        //{
        //    base.OnSerialize(info);
        //
        //    info.AddValue("OrthographicSize", _orthographicSize);
        //    info.AddValue("ZNear", _zNear);
        //    info.AddValue("ZFar", _zFar);
        //    info.AddValue("AspectRatio", _aspectRatio);
        //
        //}
        //
        //public override void OnDeserialize(SerializationInfo info)
        //{
        //    base.OnDeserialize(info);
        //
        //    _orthographicSize = info.GetValue<float>("OrthographicSize");
        //    _zNear = info.GetValue<float>("ZNear");
        //    _zFar = info.GetValue<float>("ZFar");
        //    _aspectRatio = info.GetValue<float>("AspectRatio");
        //    MarkProjectionDirty();
        //}
        //#endregion
    }
}
