using System.Numerics;

using CrossEngine.Utils.Editor;

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

                InvokeShapeChangedEvent();
            }
        }

        public override object Clone()
        {
            return new BoxColliderComponent() { Size = this.Size };
        }
    }
}
