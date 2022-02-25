using System.Numerics;

namespace CrossEngine.Components
{
    public class BoxColliderComponent : ColliderComponent
    {
        Vector3 _size = Vector3.One;
        public Vector3 Size
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
