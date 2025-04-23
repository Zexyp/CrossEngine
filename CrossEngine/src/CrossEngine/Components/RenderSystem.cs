using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Components;
using CrossEngine.Rendering;
using CrossEngine.Display;
using CrossEngine.Ecs;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;

namespace CrossEngine.Components
{
    // TODO: move stuff to scene renderer
    public class RenderSystem : Ecs.System
    {
        public Pipeline Pipeline => _pipeline;

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
        private Pipeline _pipeline;
        
        private SkyboxPass _passSkybox;
        private ScenePass _passScene;

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
            _pipeline = new Pipeline();
            _pipeline.PushBack(_passSkybox = new SkyboxPass());
            _pipeline.PushBack(_passScene = new ScenePass());
            //_pipeline.PushBack(new LightingPass());
        }

        protected internal override void OnInit()
        {
            World.Storage.MakeIndex(typeof(RendererComponent));
            World.Storage.AddNotifyRegister(typeof(CameraComponent), RegisterCamera, true);
            World.Storage.AddNotifyUnregister(typeof(CameraComponent), UnregisterCamera, true);
            World.Storage.AddNotifyRegister(typeof(SkyboxRendererComponent), RegisterSkybox);
            World.Storage.AddNotifyUnregister(typeof(SkyboxRendererComponent), UnregisterSkybox);

            _passScene.objects = new CastWrapCollection<IObjectRenderData>(World.Storage.GetIndex(typeof(RendererComponent)));
            _passScene.CameraGetter = () => OverrideCamera ?? PrimaryCamera;
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
        }

        public void GraphicsDestroy()
        {
            _pipeline.Destroy();
        }
    }
}

class SkyboxPass : Pass
{
    static Vector3[] skyboxVertices = {
        // positions
        // thx ogl tutorial
        new (-1.0f,  1.0f, -1.0f),
        new (-1.0f, -1.0f, -1.0f),
        new (1.0f, -1.0f, -1.0f),
        new (1.0f, -1.0f, -1.0f),
        new (1.0f,  1.0f, -1.0f),
        new (-1.0f,  1.0f, -1.0f),

        new (-1.0f, -1.0f,  1.0f),
        new (-1.0f, -1.0f, -1.0f),
        new (-1.0f,  1.0f, -1.0f),
        new (-1.0f,  1.0f, -1.0f),
        new (-1.0f,  1.0f,  1.0f),
        new (-1.0f, -1.0f,  1.0f),

        new (1.0f, -1.0f, -1.0f),
        new (1.0f, -1.0f,  1.0f),
        new (1.0f,  1.0f,  1.0f),
        new (1.0f,  1.0f,  1.0f),
        new (1.0f,  1.0f, -1.0f),
        new (1.0f, -1.0f, -1.0f),

        new (-1.0f, -1.0f,  1.0f),
        new (-1.0f,  1.0f,  1.0f),
        new (1.0f,  1.0f,  1.0f),
        new (1.0f,  1.0f,  1.0f),
        new (1.0f, -1.0f,  1.0f),
        new (-1.0f, -1.0f,  1.0f),

        new (-1.0f,  1.0f, -1.0f),
        new (1.0f,  1.0f, -1.0f),
        new (1.0f,  1.0f,  1.0f),
        new (1.0f,  1.0f,  1.0f),
        new (-1.0f,  1.0f,  1.0f),
        new (-1.0f,  1.0f, -1.0f),

        new (-1.0f, -1.0f, -1.0f),
        new (-1.0f, -1.0f,  1.0f),
        new (1.0f, -1.0f, -1.0f),
        new (1.0f, -1.0f, -1.0f),
        new (-1.0f, -1.0f,  1.0f),
        new (1.0f, -1.0f,  1.0f)
    };

    private const string skyboxShader = @"
#type vertex
#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projection = mat4(1);
uniform mat4 view = mat4(1);

void main()
{
    TexCoords = aPos;
    vec4 pos = projection * view * vec4(aPos, 1.0);
    gl_Position = pos.xyww;
}
#type fragment
#version 330 core
out vec4 FragColor;

in vec3 TexCoords;

uniform samplerCube skybox;

void main()
{    
    FragColor = texture(skybox, TexCoords);
}
";
    
    public ISkyboxRenderData Skybox;
    private IMesh _skyboxMesh;
    private WeakReference<ShaderProgram> _skyboxShader;

    public override void Init()
    {
        _skyboxMesh = new Mesh<Vector3>(skyboxVertices);
        _skyboxMesh.SetupGpuResources();
        using (var stream = new MemoryStream())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(skyboxShader);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            _skyboxShader = ShaderPreprocessor.CreateProgramFromStream(stream);
        }
    }

    public override void Destroy()
    {
        _skyboxMesh.FreeGpuResources();
        _skyboxShader.Dispose();
    }

    public override void Draw()
    {
        if (Skybox?.Texture == null)
            return;

        _skyboxShader.GetValue().Use();
        Skybox.Texture.GetValue().Bind();
        GraphicsContext.Current.Api.DrawArray(_skyboxMesh.VA, (uint)_skyboxMesh.Vertices.Length, DrawMode.Traingles);
    }
}

class ScenePass : Pass
{
    public IList<IObjectRenderData> objects;
    public Func<ICamera> CameraGetter;
    
    public override void Init()
    {
        foreach (var rend in _renderables.Values)
        {
            rend.Init();
        }
    }

    public override void Destroy()
    {
        foreach (var rend in _renderables.Values)
        {
            rend.Destroy();
        }
    }

    private static readonly Dictionary<Type, IRenderable> _renderables = new Dictionary<Type, IRenderable>(new InterfaceTypeComparer<IObjectRenderData>())
    {
        {typeof(ISpriteRenderData), new SpriteRenderable()},
        {typeof(IMeshRenderData), new MeshRenderable()},
    };
    
    public override void Draw()
    {
        CullVisibility();
        
        var camera = CameraGetter.Invoke();
        if (camera == null)
            return;
        
        foreach (var rend in _renderables.Values)
        {
            rend.Begin(camera);
        }
        foreach (IObjectRenderData rd in objects)
        {
            if (!rd.Visible)
                continue;

            var type = rd.GetType();
            if (!_renderables.TryGetValue(type, out var rndrbl))
            {
                Log.Default.Warn($"no usable renderable for '{type.FullName}'");
                continue;
            }
            rndrbl.Submit(rd);
        }
        foreach (var rend in _renderables.Values)
        {
            rend.End();
        }
    }

    private void CullVisibility()
    {
        // todo
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

class LightingPass : Pass
{
    public override void Draw()
    {
        throw new NotImplementedException();
    }
}
