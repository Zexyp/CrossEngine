using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Lines;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Entities.Components
{
    [RequireComponent(typeof(TransformComponent))]
    public class Box2DColliderComponent : ColliderComponent
    {
        private TransformComponent _transform;
        public TransformComponent Transform
        {
            get => _transform;
            private set
            {
                if (_transform != null) _transform.OnTransformChanged -= OnTransformChanged;
                _transform = value;
                if (_transform != null) _transform.OnTransformChanged += OnTransformChanged;
            }
        }

        private Vector2 _size = Vector2.One;

        [EditorVector2Value]
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                if (NativeShape != null)
                {
                    NativeShape.Dispose();
                    NativeShape = new Box2DShape((new Vector3(_size / 2, 1) * Vector3.Abs(Entity.Transform.WorldScale)).ToBullet());
                }
            }
        }

        public Box2DColliderComponent()
        {

        }

        public override void OnAttach()
        {
            Transform = Entity.GetComponent<TransformComponent>();
        }

        public override void OnDetach()
        {
            Transform = null;
        }

        private void OnTransformChanged(TransformComponent sender)
        {
            if (((Box2DShape)NativeShape).HalfExtentsWithMargin.ToNumerics() != new Vector3(_size / 2, 1) * Vector3.Abs(Entity.Transform.WorldScale))
            {
                NativeShape.Dispose();
                NativeShape = CreateNativeShape();
            }
        }

        public override void OnRender(RenderEvent re)
        {
            if (re is LineRenderEvent && Entity.Transform != null)
            {
                LineRenderer.DrawSquare(Matrix4x4.CreateScale(new Vector3(Size, 0)) * Entity.Transform.WorldTransformMatrix, ColliderRepresentationColor);
            }
        }

        protected override CollisionShape CreateNativeShape()
        {
            return new Box2DShape((new Vector3(_size / 2, 1) * Vector3.Abs(Entity.Transform.WorldScale)).ToBullet());
        }

        public override void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Size", Size);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            Size = (Vector2)info.GetValue("Size", typeof(Vector2));
        }
    }
}
