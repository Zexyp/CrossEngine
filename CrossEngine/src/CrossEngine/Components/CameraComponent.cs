using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Ecs;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Serialization;
using CrossEngine.Systems;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components
{
    public class CameraComponent : Component, ICamera
    {
        public Matrix4x4 ViewMatrix { get => Matrix4x4.CreateTranslation(-Entity?.Transform?.Position ?? Vector3.Zero) * Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(Entity?.Transform?.Rotation ?? Quaternion.Identity)); }
        public virtual Matrix4x4 ProjectionMatrix { get; set; } = Matrix4x4.Identity;
        public Frustum Frustum => Frustum.Create(ProjectionMatrix, ViewMatrix);

        [EditorValue]
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

        public override object Clone()
        {
            var comp = new CameraComponent();
            comp.ProjectionMatrix = this.ProjectionMatrix;
            return comp;
        }

        protected internal override void OnSerialize(SerializationInfo info)
        {
            info.AddValue(nameof(Primary), Primary);
            info.AddValue(nameof(ProjectionMatrix), ProjectionMatrix);
        }

        protected internal override void OnDeserialize(SerializationInfo info)
        {
            Primary = info.GetValue(nameof(Primary), Primary);
            ProjectionMatrix = info.GetValue(nameof(ProjectionMatrix), ProjectionMatrix);
        }
    }
}
