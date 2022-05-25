using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.ECS;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class CapsuleColliderComponent : DirectionalColliderComponent
    {
        float _radius = 0.5f;
        float _length = 1;

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

        [EditorDrag(Min = float.Epsilon)]
        public float Length
        {
            get => _length;
            set
            {
                if (value == _length) return;
                _length = value;

                NotifyShapeChangedEvent();
            }
        }

        protected override Component CreateClone()
        {
            return new CapsuleColliderComponent() {
                Radius = this.Radius,
                Length = this.Length,
            };
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            base.Serialize(info);
            info.AddValue(nameof(Radius), Radius);
            info.AddValue(nameof(Length), Length);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            base.Deserialize(info);
            Radius = info.GetValue(nameof(Radius), Radius);
            Length = info.GetValue(nameof(Length), Length);
        }
    }
}
