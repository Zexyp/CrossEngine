using System.Numerics;

namespace CrossEngine.Components
{
    public class Box2DColliderComponent : ColliderComponent
    {
        Vector2 _size = Vector2.One;
        public Vector2 Size
        {
            get => _size;
            set
            {
                if (value == _size) return;
                _size = value;

                InvokeShapeChangedEvent();
            }
        }
    }
}
