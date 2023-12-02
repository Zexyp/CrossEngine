using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Components;
using CrossEngine.Display;
using CrossEngine.Ecs;
using CrossEngine.Events;

namespace CrossEngine.Systems
{
    internal class RenderSystem : UnicastSystem<CameraComponent>
    {
        public CameraComponent? PrimaryCamera
        {
            get => _primaryCamera;
            private set
            {
                if (_primaryCamera == value)
                    return;
                _primaryCamera = value;

                if (_window != null)
                    _primaryCamera?.Resize(_window.Width, _window.Height);
                
                PrimaryCameraChanged?.Invoke(this);
            }
        }
        public Window Window
        {
            get => Window;
            set
            {
                if (_window != null) _window.Event -= Window_OnEvent;
                _window = value;
                if (_window != null)
                {
                    _primaryCamera?.Resize(_window.Width, _window.Height);
                    _window.Event += Window_OnEvent;
                }
            }
        }
        public event Action<RenderSystem> PrimaryCameraChanged;
        
        private CameraComponent? _primaryCamera = null;
        internal Window _window;

        private void Window_OnEvent(Event e)
        {
            if (e is WindowResizeEvent wre)
                _primaryCamera?.Resize(wre.Width, wre.Height);
        }

        public override void Register(CameraComponent component)
        {
            if (component.Primary)
            {
                Deprioritize(PrimaryCamera);
                PrimaryCamera = component;
            }

            component.PrimaryChanged += Camera_OnPrimaryChanged;
        }

        public override void Unregister(CameraComponent component)
        {
            component.PrimaryChanged -= Camera_OnPrimaryChanged;

            if (component == PrimaryCamera)
            {
                Deprioritize(PrimaryCamera);
                PrimaryCamera = null;
            }
        }

        private void Camera_OnPrimaryChanged(CameraComponent component)
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
            component.PrimaryChanged -= Camera_OnPrimaryChanged;
            component.Primary = false;
            component.PrimaryChanged += Camera_OnPrimaryChanged;
        }
    }
}
