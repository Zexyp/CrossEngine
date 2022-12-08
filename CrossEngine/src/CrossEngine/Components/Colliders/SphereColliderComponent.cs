using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.ECS;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class SphereColliderComponent : ColliderComponent
    {
        float _radius = 1;
        
        [EditorDrag(float.Epsilon, float.MaxValue)]
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

        protected internal override void Serialize(SerializationInfo info)
        {
            base.Serialize(info);
            info.AddValue(nameof(Radius), Radius);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            base.Deserialize(info);
            Radius = info.GetValue(nameof(Radius), 1.0f);
        }
    }
}
