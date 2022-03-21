using System.Numerics;

namespace CrossEngine.Components
{
    public class CircleColliderComponent : ColliderComponent
    {
        float _radius = 1;
        public float Radius
        {
            get => _radius;
            set
            {
                if (value == _radius) return;
                _radius = value;

                InvokeShapeChangedEvent();
            }
        }
    }
}
