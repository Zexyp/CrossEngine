using CrossEngine.Ecs;
using CrossEngine.Serialization;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Components
{
    public class PerspectiveCameraComponent : CameraComponent
    {
        public override Matrix4x4 ProjectionMatrix
        {
            get
            {
                if (_projectionDirty) UpdateProjection();
                return _projection;
            }
            set => throw new InvalidOperationException();
        }
        public float Far
        {
            get => _far;
            set
            {
                if (_far == value) return;
                _far = value;
                _projectionDirty = true;
            }
        }
        public float Near
        {
            get => _near;
            set
            {
                if (_near == value) return;
                _near = value;
                _projectionDirty = true;
            }
        }
        public float FOV
        {
            get => _fov;
            set
            {
                if (_fov == value) return;
                _fov = value;
                _projectionDirty = true;
            }
        }

        private float _far = 1;
        private float _near = -1;
        private float _fov = 90;
        private float _aspect = 1;
        private bool _projectionDirty = true;
        private Matrix4x4 _projection = Matrix4x4.Identity;

        public override void Resize(float width, float height)
        {
            _aspect = width / height;
            _projectionDirty = true;
        }

        private void UpdateProjection()
        {
            // rip depth
            // TODO: fix
            _projection = Matrix4x4.CreatePerspectiveFieldOfView(_fov, _aspect, _near, _far);
            _projectionDirty = false;
        }

        protected override Component CreateClone()
        {
            var comp = new PerspectiveCameraComponent();
            comp.Near = this.Near;
            comp.Far = this.Far;
            comp.FOV = this.FOV;
            return comp;
        }

        protected internal override void OnSerialize(SerializationInfo info)
        {
            info.AddValue(nameof(Primary), Primary);
            info.AddValue(nameof(Near), Near);
            info.AddValue(nameof(Far), Far);
            info.AddValue(nameof(FOV), FOV);
        }

        protected internal override void OnDeserialize(SerializationInfo info)
        {
            Primary = info.GetValue(nameof(Primary), Primary);
            Near = info.GetValue(nameof(Near), Near);
            Far = info.GetValue(nameof(Far), Far);
            FOV = info.GetValue(nameof(FOV), FOV);
        }
    }
}
