using System.Numerics;

using CrossEngine.Utils.Editor;
using CrossEngine.ECS;

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
    }
}
