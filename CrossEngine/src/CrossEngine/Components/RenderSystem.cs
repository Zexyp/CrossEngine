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
        private Pipeline _pipeline;
        private bool _graphicsInitialized = false;
        public bool GraphicsInitialized => _graphicsInitialized;
        
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
            _pipeline.PushBack(_passScene = new ScenePass());
            _pipeline.PushBack(_passSkybox = new SkyboxPass());
            //_pipeline.PushBack(new LightingPass());
            //_pipeline.PushBack(new TransparentPass());
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
            _passSkybox.CameraGetter = () => OverrideCamera ?? PrimaryCamera;
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

        [Obsolete("no init")]
        public void CommitRenderable(IRenderable renderable, Type key)
        {
            ScenePass._renderables.Add(key, renderable);
        }

        [Obsolete("no init")]
        public void WithdrawRenderable(IRenderable renderable)
        {
            foreach(var item in ScenePass._renderables.Where(kvp => kvp.Value == renderable).ToList())
            {
                ScenePass._renderables.Remove(item.Key);
            }
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

uniform mat4 projection;
uniform mat4 view;

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
    public Func<ICamera> CameraGetter;

    private IMesh _skyboxMesh;
    private WeakReference<ShaderProgram> _skyboxShader;

    public SkyboxPass()
    {
        Depth = DepthFunc.LessEqual;
    }

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
        _skyboxMesh.Dispose();
        _skyboxShader.Dispose();
        _skyboxMesh = null;
        _skyboxShader = null;
    }

    public override void Draw()
    {
        var camera = CameraGetter.Invoke();

        if (Skybox?.Texture == null || camera == null)
            return;
        
        var shader = _skyboxShader.GetValue();
        shader.Use();
        var view = camera.GetViewMatrix();
        view.Translation = Vector3.Zero;
        shader.SetParameterMat4("view", view);
        shader.SetParameterMat4("projection", camera.ProjectionMatrix);
        Skybox.Texture.GetValue().Bind();
        GraphicsContext.Current.Api.DrawArray(_skyboxMesh.VA, (uint)_skyboxMesh.Vertices.Length, DrawMode.Traingles);
    }
}

class ScenePass : Pass
{
    public IList<IObjectRenderData> objects;
    public Func<ICamera> CameraGetter;

    public ScenePass()
    {
        Depth = DepthFunc.Default;
    }
    
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

    internal static readonly Dictionary<Type, IRenderable> _renderables = new Dictionary<Type, IRenderable>(new InterfaceTypeComparer<IObjectRenderData>())
    {
        {typeof(ISpriteRenderData), new SpriteRenderable()},
        {typeof(IMeshRenderData), new MeshRenderable()},
    };
    
    public override void Draw()
    {
        var camera = CameraGetter.Invoke();
        if (camera == null)
            return;
        
        CullVisibility(camera);
        
        foreach (var rend in _renderables.Values)
        {
            rend.Begin(camera);
        }

        //framebuffer.GetValue().Bind();
        for (int i = 0; i < objects.Count; i++)
        {
            var rd = objects[i];
            
            if (!rd.IsVisible)
                continue;

            var type = rd.GetType();
            if (!_renderables.TryGetValue(type, out var rndrbl))
            {
                // fixme
                //Log.Default.Warn($"no usable renderable for '{type.FullName}'");
                continue;
            }
            rndrbl.Submit(rd);
        }
        //framebuffer.GetValue().Unbind();
        
        foreach (var rend in _renderables.Values)
        {
            rend.End();
        }
    }

    private void CullVisibility(ICamera camera)
    {
        var frustum = camera.GetFrustum();
        
        for (int i = 0; i < objects.Count; i++)
        {
            var obj = objects[i];
            var volume = obj.GetVolume();
            
            CullChecker.Append(volume);
            
            if (volume == null)
                continue;
            
            var result = volume.IsInFrustum(frustum);
            obj.IsVisible = result != Halfspace.Outside;
        }
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
