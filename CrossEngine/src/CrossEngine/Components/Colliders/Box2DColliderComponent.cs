using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.ECS;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class Box2DColliderComponent : ColliderComponent
    {
        Vector2 _size = Vector2.One;

        [EditorDrag(float.Epsilon, float.MaxValue)]
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

        protected internal override void Serialize(SerializationInfo info)
        {
            base.Serialize(info);
            info.AddValue(nameof(Size), Size);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            base.Deserialize(info);
            Size = info.GetValue(nameof(Size), Vector2.One);
        }
    }
}
