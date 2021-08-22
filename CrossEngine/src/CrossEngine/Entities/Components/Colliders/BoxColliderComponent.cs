using System;
using BulletSharp;

using System.Numerics;

using CrossEngine.Events;
using CrossEngine.Utils.Editor;
using CrossEngine.Rendering.Lines;
using CrossEngine.Rendering.Passes;

namespace CrossEngine.Entities.Components
{
    [RequireComponent(typeof(TransformComponent))]
    public class BoxColliderComponent : ColliderComponent
    {
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
                    Shape = new BoxShape(_size / 2 * Entity.Transform.WorldScale);
                    ShapeChanged();
                }
            }
        }

        public BoxColliderComponent()
        {

        }

        public override void OnAttach()
        {
            Shape = new BoxShape(_size / 2 * Entity.Transform.WorldScale);
        }

        public override void OnDetach()
        {
            Shape.Dispose();
            Shape = null;
        }

        public override void OnRender(RenderEvent re)
        {
            if (re is LineRenderPassEvent)
            {
                LineRenderer.DrawBox(Matrix4x4.CreateScale(Size) * Entity.Transform.WorldTransformMatrix, ColliderRepresentationColor);
            }
        }
    }
}
