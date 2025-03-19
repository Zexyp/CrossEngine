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
using CrossEngine.Rendering;
using ImGuiNET;

namespace CrossEngine.Components
{
    public class RenderSystem : CrossEngine.Ecs.System
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
        
        event Action<RenderSystem> PrimaryCameraChanged;
        
        private CameraComponent? _primaryCamera = null;
        private Vector2 _lastSize = Vector2.One;
        private ISurface _surface;

        public void SetSurface(ISurface surface)
        {
            if (_surface != null)
            {
                _surface.Resize -= OnResize;
                _surface.Update -= OnRender;
            }
            _surface = surface;
            if (_surface != null)
            {
                _surface.Resize += OnResize;
                _surface.Update += OnRender;
                _lastSize = _surface.Size;
                OnResize(_surface, _lastSize.X, _lastSize.Y);
            }
        }

        private void OnResize(ISurface sender, float width, float height)
        {
            _lastSize = new(width, height);
            _primaryCamera?.Resize(width, height);
        }

        private void OnRender(ISurface sender)
        {
            
        }

        protected internal override void OnInit()
        {
            World.Storage.AddNotifyRegister(typeof(CameraComponent), RegisterCamera, true);
            World.Storage.AddNotifyUnregister(typeof(CameraComponent), UnregisterCamera, true);
        }

        protected internal override void OnShutdown()
        {
            World.Storage.RemoveNotifyRegister(typeof(CameraComponent), RegisterCamera);
            World.Storage.RemoveNotifyUnregister(typeof(CameraComponent), UnregisterCamera);
        }

        private void RegisterCamera(Component c)
        {
            CameraComponent component = (CameraComponent)c;

            if (component.Primary)
            {
                Deprioritize(PrimaryCamera);
                PrimaryCamera = component;
            }

            component.PrimaryChanged += OnCameraPrimaryChanged;
        }

        private void UnregisterCamera(Component c)
        {
            CameraComponent component = (CameraComponent)c;

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
