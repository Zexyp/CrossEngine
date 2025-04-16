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

namespace CrossEngine.Components
{
    // TODO: move stuff to scene renderer
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
        

        public ISurface SetSurface(ISurface surface)
        {
            var old = _surface;
            
            //if (_surface != null)
            //{
            //    _surface.Resize -= OnResize;
            //    _surface.Update -= OnRender;
            //}
            //_surface = surface;
            //if (_surface != null)
            //{
            //    _surface.Resize += OnResize;
            //    _surface.Update += OnRender;
            //    if (_lastSize != _surface.Size)
            //    {
            //        _lastSize = _surface.Size;
            //        OnResize(_surface, _lastSize.X, _lastSize.Y);
            //    }
            //}
            
            return old;
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
    }

    class Pipeline
    {
        // mby surface
        List<Pass> _passes = new List<Pass>();

        public void PushBack(Pass pass)
        {
            _passes.Add(pass);
        }

        public void PushFront(Pass pass)
        {
            _passes.Insert(0, pass);
        }

        public void Process(GraphicsContext context)
        {
            Pass last = null;
            for (int i = 0; i < _passes.Count; i++)
            {
                Pass pass = _passes[i];
                
                if (last?.Depth != pass.Depth) context.Api.SetDepthFunc(pass.Depth);
                if (last?.Blend != pass.Blend) context.Api.SetBlendFunc(pass.Blend);
                if (last?.PolyMode != pass.PolyMode) context.Api.SetPolygonMode(pass.PolyMode);
                if (last?.Cull != pass.Cull) context.Api.SetCullFace(pass.Cull);

                pass.Geom();
                
                last = pass;
            }
        }
    }

    class Pass
    {
        // cull
        public DepthFunc Depth;
        public BlendFunc Blend;
        public PolygonMode PolyMode;
        public CullFace Cull;

        public void Geom() { }
    }
}
