using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Rendering;
using CrossEngine.Display;
using CrossEngine.Ecs;
using CrossEngine.Events;
using CrossEngine.Geometry;
using CrossEngine.Loaders;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;
using CrossEngine.Utils.Collections;
using CrossEngine.Utils.Extensions;
using CrossEngine.Utils.Rendering;

namespace CrossEngine.Components
{
    public class RenderSystem : Ecs.System
    {
        public Pipeline Pipeline => _pipeline;
        public Func<Action, Task> RendererRequest { get; internal set; }
        
        event Action<RenderSystem> PrimaryCameraChanged;
        
        private CameraComponent? _primaryCamera = null;
        private ICamera _overrideCamera;
        private Vector2 _lastSize = Vector2.One;
        private Pipeline _pipeline;
        private bool _graphicsInitialized = false;
        public bool GraphicsInitialized => _graphicsInitialized;
        
        private SkyboxPass _passSkybox;
        private ScenePass _passScene;
        private TransparentPass _passTransparent;

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

        //public ISurface SetSurface(ISurface surface)
        //{
        //    var old = _surface;
        //    
        //    //if (_surface != null)
        //    //{
        //    //    _surface.Resize -= OnResize;
        //    //    _surface.Update -= OnRender;
        //    //}
        //    //_surface = surface;
        //    //if (_surface != null)
        //    //{
        //    //    _surface.Resize += OnResize;
        //    //    _surface.Update += OnRender;
        //    //    if (_lastSize != _surface.Size)
        //    //    {
        //    //        _lastSize = _surface.Size;
        //    //        OnResize(_surface, _lastSize.X, _lastSize.Y);
        //    //    }
        //    //}
        //    
        //    return old;
        //}

        public RenderSystem()
        {
            _pipeline = new DeferredPipeline();
            _passScene = _pipeline.GetPass<ScenePass>();
            _passSkybox = _pipeline.GetPass<SkyboxPass>();
            _passTransparent = _pipeline.GetPass<TransparentPass>();
        }

        protected internal override void OnInit()
        {
            World.Storage.MakeIndex(typeof(RendererComponent));
            
            World.Storage.AddNotifyRegister(typeof(CameraComponent), RegisterCamera, true);
            World.Storage.AddNotifyUnregister(typeof(CameraComponent), UnregisterCamera, true);
            World.Storage.AddNotifyRegister(typeof(SkyboxRendererComponent), RegisterSkybox);
            World.Storage.AddNotifyUnregister(typeof(SkyboxRendererComponent), UnregisterSkybox);

            var coll = new CastWrapCollection<IObjectRenderData>(World.Storage.GetIndex(typeof(RendererComponent)));
            _passScene.objects = coll;
            _passTransparent.objects = coll;
            var getter = () => OverrideCamera ?? PrimaryCamera;
            _passScene.CameraGetter = getter;
            _passSkybox.CameraGetter = getter;
            _passTransparent.CameraGetter = getter;
        }

        protected internal override void OnShutdown()
        {
            World.Storage.DropIndex(typeof(RendererComponent));
            
            World.Storage.RemoveNotifyRegister(typeof(CameraComponent), RegisterCamera);
            World.Storage.RemoveNotifyUnregister(typeof(CameraComponent), UnregisterCamera);
            World.Storage.RemoveNotifyRegister(typeof(SkyboxRendererComponent), RegisterSkybox);
            World.Storage.RemoveNotifyUnregister(typeof(SkyboxRendererComponent), UnregisterSkybox);
        }

        private void RegisterCamera(Component c)
        {
            CameraComponent component = (CameraComponent)c;

            component.PrimaryChanged += OnCameraPrimaryChanged;
            
            if (component.Primary)
            {
                Deprioritize(PrimaryCamera);
                PrimaryCamera = component;
            }
        }

        private void UnregisterCamera(Component c)
        {
            CameraComponent component = (CameraComponent)c;

            if (component == PrimaryCamera)
            {
                PrimaryCamera = null;
            }

            component.PrimaryChanged -= OnCameraPrimaryChanged;
        }

        private void RegisterSkybox(Component c)
        {
            var skyboxcomp = (SkyboxRendererComponent)c;
            _passSkybox.Skybox = skyboxcomp;
        }

        private void UnregisterSkybox(Component c)
        {
            var skyboxcomp = (SkyboxRendererComponent)c;
            if (_passSkybox.Skybox == skyboxcomp)
                _passSkybox.Skybox = null;
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
        
        public void OnSurfaceResize(ISurface surface, float width, float height)
        {
            _lastSize = new(width, height);
            _primaryCamera?.Resize(width, height);
            if (_overrideCamera != null && _overrideCamera is IResizableCamera rc)
                rc.Resize(width, height);
        }

        public void GraphicsInit()
        {
            _pipeline.Init();
            _graphicsInitialized = true;
        }

        public void GraphicsDestroy()
        {
            _graphicsInitialized = false;
            _pipeline.Destroy();
        }

        public void CommitRenderable(IRenderable renderable, Type key)
        {
            RendererRequest.Invoke(renderable.Init);
            _passScene._renderables.Add(key, renderable);
        }

        public void WithdrawRenderable(IRenderable renderable)
        {
            foreach(var item in _passScene._renderables.Where(kvp => kvp.Value == renderable))
            {
                _passScene._renderables.Remove(item.Key);
                RendererRequest.Invoke(renderable.Destroy);
            }
        }
    }
}
