using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Ecs;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Systems;

namespace CrossEngine.Components
{
    internal class CameraComponent : Component
    {
        public ICamera Camera;

        public bool Primary
        {
            get => _primary;
            set
            {
                if (value == _primary)
                    return;

                _primary = value;
                PrimaryChanged?.Invoke(this);
            }
        }

        public event Action<CameraComponent> PrimaryChanged;

        private bool _primary;
    }
}
