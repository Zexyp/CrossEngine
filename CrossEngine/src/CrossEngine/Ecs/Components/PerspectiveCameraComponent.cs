﻿using CrossEngine.Components;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Ecs.Components
{
    internal class PerspectiveCameraComponent : CameraComponent
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
    }
}
