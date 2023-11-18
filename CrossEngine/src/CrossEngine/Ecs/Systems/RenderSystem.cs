using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Components;
using CrossEngine.Ecs;

namespace CrossEngine.Systems
{
    internal class RenderSystem : UnicastSystem<CameraComponent>
    {
        public CameraComponent PrimaryCamera
        {
            get => _primaryCamera;
            private set
            {
                if (_primaryCamera == value)
                    return;
                _primaryCamera = value;
                PrimaryCameraChanged?.Invoke(this);
            }
        }

        public event Action<RenderSystem> PrimaryCameraChanged;
        private CameraComponent _primaryCamera = null;

        public override void Register(CameraComponent component)
        {
            if (component.Primary)
            {
                Deprioritize(PrimaryCamera);
                PrimaryCamera = component;
            }

            component.PrimaryChanged += OnPrimaryChanged;
        }

        public override void Unregister(CameraComponent component)
        {
            component.PrimaryChanged -= OnPrimaryChanged;

            if (component == PrimaryCamera)
            {
                Deprioritize(PrimaryCamera);
                PrimaryCamera = null;
            }
        }

        private void OnPrimaryChanged(CameraComponent component)
        {
            Deprioritize(PrimaryCamera);

            if (component.Primary == false)
                PrimaryCamera = null;
            else
                PrimaryCamera = component;
        }

        private void Deprioritize(CameraComponent component)
        {
            if (component == null)
                return;
            component.PrimaryChanged -= OnPrimaryChanged;
            component.Primary = false;
            component.PrimaryChanged += OnPrimaryChanged;
        }
    }
}
