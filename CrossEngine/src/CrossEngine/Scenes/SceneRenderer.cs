using CrossEngine.Components;
using CrossEngine.Ecs;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;

namespace CrossEngine.Scenes
{
    public interface ISceneRenderData
    {
        IList<IObjectRenderData> Objects { get; }
        IList<ILightRenderData> Lights { get; }
        ISkyboxRenderData Skybox { get; }
        ICamera Camera { get; }
        Vector4? ClearColor { get; }
        Vector2 ViewportSize { get; set; }
        
    }
    
    public class SceneRenderer
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
        
        public Pipeline Pipeline;
        
        private ICamera _overrideCamera;
        private Vector2 _lastSize = Vector2.One;

        internal readonly Dictionary<Type, IRenderable> _renderables = new Dictionary<Type, IRenderable>(new InterfaceTypeComparer<IObjectRenderData>())
        {
            {typeof(ISpriteRenderData), new SpriteRenderable()},
            {typeof(IMeshRenderData), new MeshRenderable()},
        };
        
        public void Init()
        {
            Pipeline.Init();
        }

        public void Destroy()
        {
            Pipeline.Destroy();
        }
        
        public void Render(ISceneRenderData scene, ISurface surface)
        {
            surface.Context.Api.SetViewport(0, 0, (uint)surface.Size.X, (uint)surface.Size.Y);

            Pipeline.Camera = OverrideCamera ?? scene.Camera;
            scene.ViewportSize = surface.Size;
            Pipeline.Process(scene, surface);
        }
    }
}
