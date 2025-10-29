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
using CrossEngine.Rendering.Lighting;
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
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Utils.Collections;
using CrossEngine.Utils.Extensions;
using CrossEngine.Utils.Rendering;

namespace CrossEngine.Components
{
    public class RenderSystem : Ecs.System, ISceneRenderData
    {
        private CameraComponent? _primaryCamera = null;
        private bool _graphicsInitialized = false;
        public bool GraphicsInitialized => _graphicsInitialized;
        public GraphicsContext Graphics;
        
        IList<IObjectRenderData> ISceneRenderData.Objects => _objects;
        IList<ILightRenderData> ISceneRenderData.Lights => _lights;
        ISkyboxRenderData ISceneRenderData.Skybox => _skybox;
        ICamera ISceneRenderData.Camera => _primaryCam;
        
        private IList<IObjectRenderData> _objects;
        private IList<ILightRenderData> _lights;
        private ISkyboxRenderData _skybox;
        private CameraComponent _primaryCam = null;

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
        }

        protected internal override void OnInit()
        {
            World.Storage.MakeIndex(typeof(RendererComponent));
            World.Storage.MakeIndex(typeof(LightComponent));
            
            World.Storage.AddNotifyRegister(typeof(CameraComponent), RegisterCamera, true);
            World.Storage.AddNotifyUnregister(typeof(CameraComponent), UnregisterCamera, true);
            World.Storage.AddNotifyRegister(typeof(SkyboxRendererComponent), RegisterSkybox);
            World.Storage.AddNotifyUnregister(typeof(SkyboxRendererComponent), UnregisterSkybox);

            _objects = new CastWrapCollection<IObjectRenderData>(World.Storage.GetIndex(typeof(RendererComponent)));
            _lights = new CastWrapCollection<ILightRenderData>(World.Storage.GetIndex(typeof(LightComponent)));
        }

        protected internal override void OnShutdown()
        {
            World.Storage.DropIndex(typeof(RendererComponent));
            World.Storage.DropIndex(typeof(LightComponent));

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
                Deprioritize(_primaryCam);
                _primaryCam = component;
            }
        }

        private void UnregisterCamera(Component c)
        {
            CameraComponent component = (CameraComponent)c;

            if (component == _primaryCam)
            {
                _primaryCam = null;
            }

            component.PrimaryChanged -= OnCameraPrimaryChanged;
        }

        private void RegisterSkybox(Component c)
        {
            var skyboxcomp = (SkyboxRendererComponent)c;
            _skybox = skyboxcomp;
        }

        private void UnregisterSkybox(Component c)
        {
            var skyboxcomp = (SkyboxRendererComponent)c;
            if (_skybox == skyboxcomp)
                _skybox = null;
        }

        private void OnCameraPrimaryChanged(CameraComponent component)
        {
            Deprioritize(_primaryCam);

            if (component.Primary == false)
                _primaryCam = null;
            else
                _primaryCam = component;
        }

        private void Deprioritize(CameraComponent component)
        {
            if (component == null)
                return;
            component.PrimaryChanged -= OnCameraPrimaryChanged;
            component.Primary = false;
            component.PrimaryChanged += OnCameraPrimaryChanged;
        }
        
        public void ProcessSurfaceResize(ISurface surface, float width, float height)
        {
            _primaryCamera?.Resize(width, height);
        }

        /*
        public void CommitRenderable(IRenderable renderable, Type key)
        {
            RendererRequest.Invoke(renderable.Init);
            Renderables.Add(key, renderable);
        }

        public void WithdrawRenderable(IRenderable renderable)
        {
            foreach(var item in Renderables.Where(kvp => kvp.Value == renderable))
            {
                Renderables.Remove(item.Key);
                RendererRequest.Invoke(renderable.Destroy);
            }
        }
        */
    }
}
