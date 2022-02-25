using System;

using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;

namespace CrossEngine.Components
{
    public abstract class ColliderComponent : Component
    {
        internal event Action<ColliderComponent> OnShapeChanged;

        //public Vector3 PositionOffset = Vector3.Zero;
        //public Quaternion RotationOffset = Quaternion.Identity;

        public Matrix4x4 LocalOffset = Matrix4x4.Identity;

        protected void InvokeShapeChangedEvent()
        {
            OnShapeChanged?.Invoke(this);
        }

        public override void Attach()
        {
            PhysicsSysten.Instance.RegisterCollider(this);
        }

        public override void Detach()
        {
            PhysicsSysten.Instance.UnregisterCollider(this);
        }
    }
}
