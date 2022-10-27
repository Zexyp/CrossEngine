using System;

using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public enum ColliderDirection
    {
        X,
        Y,
        Z
    }

    public abstract class DirectionalColliderComponent : ColliderComponent
    {
        protected ColliderDirection _direction = ColliderDirection.Y;

        [EditorEnum]
        public ColliderDirection Direction
        {
            get => _direction;
            set
            {
                if (value == _direction) return;
                _direction = value;

                NotifyShapeChangedEvent();
            }
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            base.Serialize(info);
            info.AddValue(nameof(Direction), Direction);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            base.Deserialize(info);
            Direction = info.GetValue(nameof(Direction), Direction);
        }
    }
}
