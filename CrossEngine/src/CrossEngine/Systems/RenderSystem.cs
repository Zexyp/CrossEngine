using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Components;
using CrossEngine.Display;
using CrossEngine.Ecs;
using CrossEngine.Events;

namespace CrossEngine.Systems
{
    public class RenderSystem : UnicastSystem<CameraComponent>
    {
        public CameraComponent? PrimaryCamera
        {
            get => _primaryCamera;
            private set
            {
                if (_primaryCamera == value)
                    return;
                _primaryCamera = value;

                _primaryCamera?.Resize(_lastSize.X, _lastSize.Y);
                
                PrimaryCameraChanged?.Invoke(this);
            }
        }
        
        public event Action<RenderSystem> PrimaryCameraChanged;
        
        private CameraComponent? _primaryCamera = null;
        private Vector2 _lastSize = Vector2.One;

        public void Resize(float width, float height)
        {
            _lastSize = new(width, height);
            _primaryCamera?.Resize(width, height);
        }

        public override void Register(CameraComponent component)
        {
            if (component.Primary)
            {
                Deprioritize(PrimaryCamera);
                PrimaryCamera = component;
            }

            component.PrimaryChanged += OnCameraPrimaryChanged;
        }

        public override void Unregister(CameraComponent component)
        {
            component.PrimaryChanged -= OnCameraPrimaryChanged;

            if (component == PrimaryCamera)
            {
                PrimaryCamera = null;
            }
        }

        private void OnCameraPrimaryChanged(CameraComponent component)
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
            component.PrimaryChanged -= OnCameraPrimaryChanged;
            component.Primary = false;
            component.PrimaryChanged += OnCameraPrimaryChanged;
        }
    }
}
