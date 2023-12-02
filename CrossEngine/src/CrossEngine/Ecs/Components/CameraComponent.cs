using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Ecs;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Systems;

namespace CrossEngine.Components
{
    internal class CameraComponent : Component, ICamera
    {
        public Matrix4x4 ViewMatrix { get => Matrix4x4.CreateTranslation(-Entity?.Transform?.Position ?? Vector3.Zero) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Entity?.Transform?.Rotation ?? Quaternion.Identity)); }
        public Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
        public Frustum Frustum => Frustum.Create(ProjectionMatrix, ViewMatrix);

        public bool Primary
        {
            get => _primary;
            set
            {
                if (value == _primary)
                    return;

                _primary = value;
                PrimaryChanged?.Invoke(this);
            }
        }

        public event Action<CameraComponent> PrimaryChanged;

        private bool _primary;

        public virtual void Resize(float width, float height) { }
    }
}
