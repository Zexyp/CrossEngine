using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using CrossEngine.Geometry;
using CrossEngine.Loaders;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering.Shaders;
using CrossEngine.Utils;
using CrossEngine.Utils.Extensions;
using CrossEngine.Utils.Rendering;

namespace CrossEngine.Rendering;

public class DeferredPipeline : Pipeline
{
    public DeferredPipeline()
    {
        PushBack(new ScenePass());
        PushBack(new LightingPass());
        PushBack(new SkyboxPass());
        PushBack(new TransparentPass());
    }

    protected override void OnInit()
    {
        var spec = new FramebufferSpecification();
        spec.Attachments = new FramebufferAttachmentSpecification(
            // using floating point colors
            new FramebufferTextureSpecification(TextureFormat.ColorRGBA16F), // color
            new FramebufferTextureSpecification(TextureFormat.ColorR32I), // id
            new FramebufferTextureSpecification(TextureFormat.ColorRGBA16F), // position
            new FramebufferTextureSpecification(TextureFormat.ColorRGBA16F), // normal
            new FramebufferTextureSpecification(TextureFormat.Depth24Stencil8)
        );
        spec.Width = 1;
        spec.Height = 1;

        Buffer = Framebuffer.Create(spec);
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

    private MeshRenderer _skyboxMeshRenderer;
    private WeakReference<ShaderProgram> _skyboxShader;

    public SkyboxPass()
    {
        Depth = DepthFunc.LessEqual;
    }

    public override void Init()
    {
        _skyboxMeshRenderer = new MeshRenderer();
        _skyboxMeshRenderer.Setup(new Mesh<Vector3>(skyboxVertices));
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
        _skyboxMeshRenderer.Dispose();
        _skyboxShader.Dispose();
        _skyboxMeshRenderer = null;
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
        _skyboxMeshRenderer.Draw(GraphicsContext.Current.Api);
    }
}

class ScenePass : Pass
{
    public IList<IObjectRenderData> objects;
    public Func<ICamera> CameraGetter;
    public int TransparentIndex = 0;

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

    internal readonly Dictionary<Type, IRenderable> _renderables = new Dictionary<Type, IRenderable>(new InterfaceTypeComparer<IObjectRenderData>())
    {
        {typeof(ISpriteRenderData), new SpriteRenderable()},
        {typeof(IMeshRenderData), new MeshRenderable()},
    };
    
    public override void Draw()
    {
        var camera = CameraGetter.Invoke();
        if (camera == null)
            return;
        
        FilterTransparent();
        
        FrustumCulling(camera.GetFrustum());
        
        DrawObjects(camera, objects, 0, TransparentIndex);
    }

    private void FrustumCulling(in Frustum frustum)
    {
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
    
    private void FilterTransparent()
    {
        if (objects is not IList) throw new InvalidOperationException();
        
        ArrayList.Adapter((IList)objects).Sort(new ComparisonComparer<IObjectRenderData>((o1, o2) =>
        {
            return IsTransparet(o1).CompareTo(IsTransparet(o2));
        }));

        TransparentIndex = FindFirstIndex(objects, o => IsTransparet(o));
        TransparentIndex = TransparentIndex == -1 ? objects.Count : TransparentIndex;
    }
    
    private static int FindFirstIndex<T>(IList<T> list, Predicate<T> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
                return i;
        }
        return -1;
    }

    private bool IsTransparet(IObjectRenderData obj)
    {
        switch (obj)
        {
            case ISpriteRenderData sprite: return sprite.Blend == BlendMode.Blend || sprite.Blend == BlendMode.Blend;
            default: return false;
        }
    }

    internal void DrawObjects(ICamera camera, IList<IObjectRenderData> objects, int rangeStart, int rangeEnd)
    {
        Debug.Assert(rangeEnd >= rangeStart);
        
        // i guess, crazy džibriš
        foreach (var group in objects
                     .Skip(rangeStart)
                     .Take(rangeEnd - rangeStart)
                     .GroupBy(item => item.GetType(), new InterfaceTypeComparer<IObjectRenderData>()))
        {
            if (!_renderables.TryGetValue(group.Key, out var rndrbl))
            {
                // fixme
                //Log.Default.Warn($"no usable renderable for '{type.FullName}'");
                continue;
            }
            
            rndrbl.Begin(camera);

            foreach (var rd in group)
            {
                if (!rd.IsVisible)
                    continue;

                var type = rd.GetType();
                
                rndrbl.Submit(rd);
            }
            
            rndrbl.End();
        }
        
        /*
        foreach (var rend in _renderables.Values)
        {
            rend.Begin(camera);
        }

        //framebuffer.GetValue().Bind();
        for (int i = rangeStart; i < rangeEnd; i++)
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
        */
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

class TransparentPass : Pass
{
    public IList<IObjectRenderData> objects;
    public Func<ICamera> CameraGetter;

    public TransparentPass()
    {
        Depth = DepthFunc.Default;
    }
    
    public override void Draw()
    {
        var camera = CameraGetter.Invoke();
        if (camera == null)
            return;

        var ti = Pipeline.GetPass<ScenePass>().TransparentIndex;
        SortByDistance(camera, ti);
        
        Pipeline.GetPass<ScenePass>().DrawObjects(camera, objects, ti, objects.Count);
    }

    private void SortByDistance(ICamera camera, int indexStart)
    {
        var cameraPos = camera.GetViewMatrix().Translation;
        
        ArrayList.Adapter((IList)objects).Sort(indexStart, objects.Count - indexStart, new ComparisonComparer<IObjectRenderData>((o1, o2) =>
        {
            var d1 = Vector3.DistanceSquared(cameraPos, o1.Transform.Translation);
            var d2 = Vector3.DistanceSquared(cameraPos, o2.Transform.Translation);
           return d1.CompareTo(d2);
        }));
    }
}

class LightingPass : Pass
{
    public override void Draw()
    {
        //throw new NotImplementedException();
    }
}