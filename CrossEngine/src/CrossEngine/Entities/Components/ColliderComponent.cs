using System;
using BulletSharp;

using System.Numerics;

namespace CrossEngine.Entities.Components
{
    public abstract class ColliderComponent : Component
    {
        public static Vector4 ColliderRepresentationColor = new Vector4(1.0f, 0.4f, 0.0f, 1.0f);

        public event Action<ColliderComponent> OnShapeChanged;

        private CollisionShape _shape;
        internal CollisionShape NativeShape
        {
            get
            {
                if (_shape == null && Enabled)
                {
                   SetupShape();
                }
                return _shape;
            }
            set
            {
                _shape = value;
                OnShapeChanged?.Invoke(this);
            }
        }

        public override void OnStart()
        {
            SetupShape();
        }

        public override void OnEnd()
        {
            _shape?.Dispose();
            _shape = null;
        }

        private void SetupShape()
        {
            _shape = CreateNativeShape();
            _shape.UserObject = this;
        }

        protected abstract CollisionShape CreateNativeShape(); 
    }
}
