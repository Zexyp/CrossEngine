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
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;
using ImGuiNET;

namespace CrossEngine.Components
{
    public class RenderSystem : CrossEngine.Ecs.System
    {
        public IResizableCamera OverrideCamera
        {
            get => _overrideCamera;
            set
            {
                _overrideCamera = value;
                _overrideCamera?.Resize(_lastSize.X, _lastSize.Y);
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
        private IResizableCamera _overrideCamera;
        private Vector2 _lastSize = Vector2.One;
        private ISurface _surface;

        internal RendererApi rapi;

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
            _overrideCamera?.Resize(width, height);
        }

        private void OnRender(ISurface sender)
        {
            rapi.Clear();
            var camera = OverrideCamera ?? PrimaryCamera;
            var srenderable = new SpriteRenderable();
            srenderable.Begin(camera);
            var mrenderable = new MeshRenderable() { RApi = rapi };
            mrenderable.Begin(camera);
            foreach (ISpriteRenderData src in World.Storage.IterateIndex(new[] { typeof(ISpriteRenderData) }).Select(v => v[0]))
            {
                srenderable.Submit(src);
            }
            foreach (IMeshRenderData mrc in World.Storage.IterateIndex(new[] { typeof(IMeshRenderData) }).Select(v => v[0]))
            {
                mrenderable.Submit(mrc);
            }
            srenderable.End();
            mrenderable.End();
        }

        protected internal override void OnInit()
        {
            World.Storage.MakeIndex(new[] { typeof(ISpriteRenderData) });
            World.Storage.MakeIndex(new[] { typeof(IMeshRenderData) });
            World.Storage.AddNotifyRegister(typeof(CameraComponent), RegisterCamera, true);
            World.Storage.AddNotifyUnregister(typeof(CameraComponent), UnregisterCamera, true);
        }

        protected internal override void OnShutdown()
        {
            World.Storage.DropIndex(new[] { typeof(ISpriteRenderData) });
            World.Storage.DropIndex(new[] { typeof(IMeshRenderData) });
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
