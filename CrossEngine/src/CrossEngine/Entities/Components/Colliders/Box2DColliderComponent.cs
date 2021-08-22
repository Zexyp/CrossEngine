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

namespace CrossEngine.Entities.Components
{
    [RequireComponent(typeof(TransformComponent))]
    public class Box2DColliderComponent : ColliderComponent
    {
        private Vector2 _size = Vector2.One;

        [EditorVector2Value]
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                if (Shape != null)
                {
                    Shape.Dispose();
                    Shape = new Box2DShape(new Vector3(_size / 2, 1) * Entity.Transform.WorldScale);
                    ShapeChanged();
                }
            }
        }

        public Box2DColliderComponent()
        {

        }

        public override void OnAttach()
        {
            Shape = new Box2DShape(new Vector3(_size / 2, 1) * Entity.Transform.WorldScale);
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
                LineRenderer.DrawSquare(Matrix4x4.CreateScale(new Vector3(Size, 0)) * Entity.Transform.WorldTransformMatrix, ColliderRepresentationColor);
            }
        }
    }
}
