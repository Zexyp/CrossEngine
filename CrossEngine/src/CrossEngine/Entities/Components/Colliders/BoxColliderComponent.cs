using System;
using BulletSharp;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.Utils;

namespace CrossEngine.Entities.Components
{
    public class BoxColliderComponent : ColliderComponent
    {
        private Vector3 _size = Vector3.One;
        public Vector3 Size
        {
            get => _size;
            set
            {
                _size = value;
                if (Shape != null)
                    ;//((BoxShape)Shape)..HalfExtentsWithoutMargin = _size; // TODO: fix!
            }
        }

        public BoxColliderComponent()
        {

        }

        public override void OnAttach()
        {
            Shape = new BoxShape(_size / 2);
        }

        public override void OnDetach()
        {
            Shape.Dispose();
            Shape = null;
        }
    }
}
