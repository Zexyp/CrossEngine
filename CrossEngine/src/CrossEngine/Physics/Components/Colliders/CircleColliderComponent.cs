using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.ECS;

namespace CrossEngine.Components
{
    public class SphereColliderComponent : ColliderComponent
    {
        float _radius = 1;
        
        [EditorDrag(Min = float.Epsilon)]
        public float Radius
        {
            get => _radius;
            set
            {
                if (value == _radius) return;
                _radius = value;

                NotifyShapeChangedEvent();
            }
        }

        protected override Component CreateClone()
        {
            return new SphereColliderComponent() { Radius = this.Radius };
        }
    }
}
