using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.ECS;

namespace CrossEngine.Components
{
    public class Box2DColliderComponent : ColliderComponent
    {
        Vector2 _size = Vector2.One;

        [EditorDrag(Min = float.Epsilon)]
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (value == _size) return;
                _size = value;

                NotifyShapeChangedEvent();
            }
        }

        protected override Component CreateClone()
        {
            return new Box2DColliderComponent() { Size = this.Size };
        }
    }
}
