using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Rendering.Passes;
using CrossEngine.Rendering.Lines;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization.Json;

namespace CrossEngine.Entities.Components
{
    [RequireComponent(typeof(TransformComponent))]
    public class BoxColliderComponent : ColliderComponent, ISerializable
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

        private Vector3 _size = Vector3.One;

        [EditorVector3Value]
        public Vector3 Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                if (Shape != null)
                {
                    Shape.Dispose();
                    Shape = new Box2DShape(_size / 2 * Vector3.Abs(Entity.Transform.WorldScale));
                }
            }
        }

        public BoxColliderComponent()
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

        public override void OnEnable()
        {
            Shape = CreateShape();
        }

        public override void OnDisable()
        {
            Shape.Dispose();
            Shape = null;
        }

        private void OnTransformChanged(TransformComponent sender)
        {
            if (((BoxShape)Shape).HalfExtentsWithMargin != _size / 2 * Vector3.Abs(Entity.Transform.WorldScale))
            {
                Shape.Dispose();
                Shape = CreateShape();
            }
        }

        public override void OnRender(RenderEvent re)
        {
            if (re is LineRenderPassEvent)
            {
                LineRenderer.DrawBox(Matrix4x4.CreateScale(Size) * Entity.Transform.WorldTransformMatrix, ColliderRepresentationColor);
            }
        }

        protected override CollisionShape CreateShape()
        {
            return new BoxShape(_size / 2 * Vector3.Abs(Entity.Transform.WorldScale));
        }

        #region ISerializable
        public void GetObjectData(SerializationInfo info)
        {
            info.AddValue("Size", Size);
        }

        public BoxColliderComponent(DeserializationInfo info)
        {
            Size = (Vector3)info.GetValue("Size", typeof(Vector3));
        }
        #endregion
    }
}
