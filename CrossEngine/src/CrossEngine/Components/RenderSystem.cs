using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Components;
using CrossEngine.Display;
using CrossEngine.Ecs;
using CrossEngine.Events;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Components
{
    public class RenderSystem : CrossEngine.Ecs.System
    {
        public ICamera OverrideCamera
        {
            get => _overrideCamera;
            set
            {
                _overrideCamera = value;
                if (_overrideCamera != null && _overrideCamera is IResizableCamera rc)
                    rc.Resize(_lastSize.X, _lastSize.Y);
            }
        }

        public CameraComponent? PrimaryCamera
        {
            get => _primaryCamera;
            private set
            {
                _primaryCamera = value;

                _primaryCamera?.Resize(_lastSize.X, _lastSize.Y);
                
                PrimaryCameraChanged?.Invoke(this);
            }
        }
        
        event Action<RenderSystem> PrimaryCameraChanged;
        
        private CameraComponent? _primaryCamera = null;
        private ICamera _overrideCamera;
        private Vector2 _lastSize = Vector2.One;
        private ISurface _surface;
        private readonly Dictionary<Type, IRenderable> _renderables = new Dictionary<Type, IRenderable>(new InterfaceTypeComparer<IObjectRenderData>())
        {
            {typeof(ISpriteRenderData), new SpriteRenderable()},
            {typeof(IMeshRenderData), new MeshRenderable()},
        };

        public ISurface SetSurface(ISurface surface)
        {
            var old = _surface;
            
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
                if (_lastSize != _surface.Size)
                {
                    _lastSize = _surface.Size;
                    OnResize(_surface, _lastSize.X, _lastSize.Y);
                }
            }
            
            return old;
        }

        private void OnResize(ISurface sender, float width, float height)
        {
            _lastSize = new(width, height);
            _primaryCamera?.Resize(width, height);
            if (_overrideCamera != null && _overrideCamera is IResizableCamera rc)
                rc.Resize(width, height);
        }

        private void OnRender(ISurface sender)
        {
            _surface.Context.Api.SetViewport(0, 0, (uint)_surface.Size.X, (uint)_surface.Size.Y);
            
            var camera = OverrideCamera ?? PrimaryCamera;
            if (camera == null)
                return;

            foreach (var rend in _renderables.Values)
            {
                rend.Begin(camera);
            }
            foreach (IObjectRenderData rd in World.Storage.GetIndex(typeof(RendererComponent)))
            {
                _renderables[rd.GetType()].Submit(rd);
            }
            foreach (var rend in _renderables.Values)
            {
                rend.End();
            }
        }

        protected internal override void OnInit()
        {
            World.Storage.MakeIndex(typeof(RendererComponent));
            World.Storage.AddNotifyRegister(typeof(CameraComponent), RegisterCamera, true);
            World.Storage.AddNotifyUnregister(typeof(CameraComponent), UnregisterCamera, true);
        }

        protected internal override void OnShutdown()
        {
            World.Storage.DropIndex(typeof(RendererComponent));
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

        // pot of boiling shit
        private class InterfaceTypeComparer<T> : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                if (x == y) return true;
                if (x.IsAssignableFrom(y)) return true;
                return false;
            }

            public int GetHashCode(Type obj)
            {
                if (obj.IsInterface)
                    return obj.GetHashCode();

                Type baseInterface = null;
                var ints = obj.GetInterfaces();
                for (int i = ints.Length - 1; i >= 0; i--)
                {
                    if (!typeof(T).IsAssignableFrom(ints[i]))
                        continue;

                    baseInterface = ints[i];
                    break;
                }
                return baseInterface?.GetHashCode() ?? obj.GetHashCode();
            }
        }
    }

    class Pipeline
    {
        List<Pass> _passes = new List<Pass>();

        public void PushBack(Pass pass)
        {
            _passes.Add(pass);
        }

        public void PushFront(Pass pass)
        {
            _passes.Insert(0, pass);
        }
    }

    class Pass
    {
        // cull
        // depth
    }
}
