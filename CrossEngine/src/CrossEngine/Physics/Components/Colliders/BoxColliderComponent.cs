using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.ECS;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class BoxColliderComponent : ColliderComponent
    {
        Vector3 _size = Vector3.One;

        [EditorDrag(Min = float.Epsilon)]
        public Vector3 Size
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
            return new BoxColliderComponent() { Size = this.Size };
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            base.Serialize(info);
            info.AddValue(nameof(Size), Size);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            base.Deserialize(info);
            Size = info.GetValue(nameof(Size), Vector3.One);
        }
    }
}
