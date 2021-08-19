using System;
using BulletSharp;

using System.Numerics;

namespace CrossEngine.Entities.Components
{
    public abstract class ColliderComponent : Component
    {
        public static Vector4 ColliderRepresentationColor = new Vector4(1.0f, 0.4f, 0.0f, 1.0f);

        private CollisionShape _shape;
        internal CollisionShape Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                OnShapeChanged?.Invoke(this);
            }
        }

        public event Action<ColliderComponent> OnShapeChanged;

        protected void ShapeChanged() => OnShapeChanged?.Invoke(this);
    }
}
