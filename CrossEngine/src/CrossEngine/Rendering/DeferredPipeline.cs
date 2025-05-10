using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using CrossEngine.Rendering.Lighting;
using CrossEngine.FX.Particles;
using CrossEngine.Geometry;
using CrossEngine.Loaders;
using CrossEngine.Platform.OpenGL;
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
using CrossEngine.Utils.Structs;
using System.Drawing.Text;

namespace CrossEngine.Rendering;

public class DeferredPipeline : Pipeline
{
    public const int AttachmentIndexColor = 0;
    public const int AttachmentIndexId = 1;
    public const int AttachmentIndexPosition = 2;
    public const int AttachmentIndexNormal = 3;
    
    public DeferredPipeline()
    {
        var scene = new ScenePass();
        PushBack(scene);
        PushBack(new LightPass());
        PushBack(new SkyboxPass());
        PushBack(new TransparentPass(scene));
        // TODO: bloom
        PushBack(new FogPass());
    }

    protected override void OnInit()
    {
        var spec = new FramebufferSpecification();
        spec.Attachments = new FramebufferAttachmentSpecification(
            // using floating point colors
            new FramebufferTextureSpecification(TextureFormat.ColorRGBA16F), // color + specular
            new FramebufferTextureSpecification(TextureFormat.ColorR32I), // id
            new FramebufferTextureSpecification(TextureFormat.ColorRGB16F), // position
            new FramebufferTextureSpecification(TextureFormat.ColorRGB16F), // normal
            new FramebufferTextureSpecification(TextureFormat.Depth24Stencil8)
        );
        spec.Width = 1;
        spec.Height = 1;

        Buffer = Framebuffer.Create(spec);
    }

    protected override void OnBeforePasses()
    {
        var buffer = Buffer.GetValue();
        buffer.EnableColorAttachments([AttachmentIndexId, AttachmentIndexPosition, AttachmentIndexNormal]);
        buffer.ClearAttachment(AttachmentIndexId, IntVec4.Zero);
        buffer.ClearAttachment(AttachmentIndexPosition, Vector4.Zero);
        buffer.ClearAttachment(AttachmentIndexNormal, Vector4.Zero);
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
layout (location = 0) in vec3 aPosition;

out vec3 vTexCoord;

uniform mat4 uProjection = mat4(1);
uniform mat4 uView = mat4(1);
uniform mat4 uModel;

void main()
{
    vTexCoord = (uModel * vec4(aPosition, 0.0)).xyz;

    vec4 pos = uProjection * uView * vec4(aPosition, 1.0);

    gl_Position = pos.xyww;
}
#type fragment
#version 330 core
in vec3 vTexCoord;
out vec4 oFragColor;

uniform samplerCube uSkybox;

void main()
{    
    oFragColor = texture(uSkybox, vTexCoord);
}
";
    
    public ISkyboxRenderData Skybox;

    private MeshRenderer _skyboxMeshRenderer;
    private WeakReference<ShaderProgram> _skyboxShader;

    public SkyboxPass()
    {
        Depth = DepthFunc.LessEqual;
    }

    public override void Init()
    {
        _skyboxMeshRenderer = new MeshRenderer();
        _skyboxMeshRenderer.Setup(MeshGenerator.GenerateCube(new Vector3(2)));
        _skyboxShader = ShaderPreprocessor.CreateProgramFromString(skyboxShader);
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
        if (Skybox?.Texture == null)
            return;
        
        var shader = _skyboxShader.GetValue();
        shader.Use();
        var view = Pipeline.Camera.GetViewMatrix();
        view.Translation = Vector3.Zero;
        shader.SetParameterMat4("uView", view);
        shader.SetParameterMat4("uProjection", Pipeline.Camera.ProjectionMatrix);
        shader.SetParameterMat4("uModel", Skybox.Transform);
        Skybox.Texture.GetValue().Bind();
        _skyboxMeshRenderer.Draw(GraphicsContext.Current.Api);
    }
}

class ScenePass : Pass
{
    public IList<IObjectRenderData> objects;
    private int _transparentIndex;

    public ScenePass()
    {
        ModifyAttachments = [DeferredPipeline.AttachmentIndexColor, DeferredPipeline.AttachmentIndexId, DeferredPipeline.AttachmentIndexNormal, DeferredPipeline.AttachmentIndexPosition];
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
        var transparentIndex = FilterTransparent();
        
        FrustumCulling(Pipeline.Camera.GetFrustum());
        
        SortByDistance(Pipeline.Camera, transparentIndex);
        
        DrawObjects(Pipeline.Camera, objects, 0, transparentIndex);
        
        _transparentIndex = transparentIndex;
    }

    public void DrawTransparent()
    {
        DrawObjects(Pipeline.Camera, objects, _transparentIndex, objects.Count);
    }
    
    private void SortByDistance(ICamera camera, int indexStart)
    {
        var cameraPos = Matrix4x4Extension.SafeInvert(camera.GetViewMatrix()).Translation;
        
        ArrayList.Adapter((IList)objects).Sort(indexStart, objects.Count - indexStart, new ComparisonComparer<IObjectRenderData>((o1, o2) =>
        {
            var d1 = Vector3.DistanceSquared(cameraPos, o1.Transform.Translation);
            var d2 = Vector3.DistanceSquared(cameraPos, o2.Transform.Translation);
            return d2.CompareTo(d1);
        }));
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
    
    private int FilterTransparent()
    {
        if (objects is not IList) throw new InvalidOperationException();
        
        ArrayList.Adapter((IList)objects).Sort(new ComparisonComparer<IObjectRenderData>((o1, o2) =>
        {
            return IsTransparet(o1).CompareTo(IsTransparet(o2));
        }));

        var index = FindFirstIndex(objects, o => IsTransparet(o));
        return index == -1 ? objects.Count : index;
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
            case ISpriteRenderData sprite: return sprite.Blend == BlendMode.Blend || sprite.Blend == BlendMode.Add;
            case IParticleSystemRenderData particleSystem: return particleSystem.Blend == BlendMode.Blend || particleSystem.Blend == BlendMode.Add;
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

    private ScenePass _scenePass;
    
    public TransparentPass(ScenePass scene)
    {
        _scenePass = scene;
        ModifyAttachments = [DeferredPipeline.AttachmentIndexColor, DeferredPipeline.AttachmentIndexId];
        Depth = DepthFunc.Default;
    }
    
    public override void Draw()
    {
        _scenePass.DrawTransparent();
    }
}

class LightPass : Pass
{
    public IList<ILightRenderData> lights;
    
    WeakReference<ShaderProgram> _shader;
    MeshRenderer _plane;

    public LightPass()
    {
        Blend = BlendFunc.One;
        Depth = DepthFunc.None;
        DepthMask = false;
    }

    const int LightBatchBufferSize = 16;
    
    private static readonly string ShaderSource = $@"
#type vertex
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

void main()
{{
    vTexCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0);
}}
#type fragment
#version 330 core
in vec2 vTexCoord;
out vec4 oFragColor;

#include ""internal:lights.glsl""

uniform sampler2D uColor;
uniform sampler2D uPosition;
uniform sampler2D uNormal;
uniform vec3 uViewPosition;
uniform vec3 uAmbient;

uniform int uNumPointLights;
uniform PointLight uPointLights[{LightBatchBufferSize}];
uniform int uNumSpotLights;
uniform SpotLight uSpotLights[{LightBatchBufferSize}];
uniform int uNumDirectionalLights;
uniform DirectionalLight uDirectionalLights[{LightBatchBufferSize}];

void main()
{{
    vec3 color = texture(uColor, vTexCoord).xyz;
    vec3 normal = normalize(texture(uNormal, vTexCoord).xyz);
    vec3 position = texture(uPosition, vTexCoord).xyz;
    float specularHighlight = texture(uColor, vTexCoord).w;
    float specularExponent = 256;

    vec3 lighting = color * uAmbient;

    for (int i = 0; i < uNumPointLights; i++) {{
        float distance = length(uPointLights[i].Position - position);
        //if(distance > uPointLights[i].Radius) // radius clip control
        //    continue;

        vec3 lightDir = normalize(uPointLights[i].Position - position);
        // diffuse
        float diffuse = max(dot(normal, lightDir), 0.0);
        // specular (blinn-phong)
        vec3 viewDir = normalize(uViewPosition - position);
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specular = pow(max(dot(normal, halfwayDir), 0.0), specularExponent);

        float attenuation = 1.0 / (1.0 + uPointLights[i].Linear * distance + uPointLights[i].Quadratic * distance * distance);

        diffuse *= attenuation;
        specular *= attenuation;
        lighting += diffuse * color * uPointLights[i].Color;
        lighting += specular * uPointLights[i].Color * specularHighlight;
    }}

    for (int i = 0; i < uNumSpotLights; i++) {{
        float distance = length(uSpotLights[i].Position - position);
        //if(distance > uSpotLights[i].Radius) // radius clip control
        //    continue;

        vec3 lightDir = normalize(uSpotLights[i].Position - position);
        // diffuse
        float diffuse = max(dot(normal, lightDir), 0.0);
        // specular (blinn-phong)
        vec3 viewDir = normalize(uViewPosition - position);
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specular = pow(max(dot(normal, halfwayDir), 0.0), specularExponent);

        float attenuation = 1.0 / (1.0 + uSpotLights[i].Linear * distance + uSpotLights[i].Quadratic * distance * distance);

        // calculate angle
        float factor = dot(lightDir, normalize(-uSpotLights[i].Direction));
        float cosAngle = cos(uSpotLights[i].Angle / 2);
        float intensity = smoothstep(cosAngle, cosAngle + uSpotLights[i].Blend, factor);
            
        diffuse *= attenuation * intensity;
        specular *= attenuation * intensity;
        lighting += diffuse * color * uSpotLights[i].Color;
        lighting += specular * uSpotLights[i].Color * specularHighlight;
    }}

    for (int i = 0; i < uNumDirectionalLights; i++) {{
        vec3 lightDir = normalize(-uDirectionalLights[i].Direction);
        // diffuse
        float diffuse = max(dot(normal, lightDir), 0.0);
        // specular (blinn-phong)
        vec3 viewDir = normalize(uViewPosition - position);
        vec3 halfwayDir = normalize(lightDir + viewDir);
        float specular = pow(max(dot(normal, halfwayDir), 0.0), specularExponent);

        lighting += diffuse * color * uDirectionalLights[i].Color;
        lighting += specular * uDirectionalLights[i].Color * specularHighlight;
    }}

    oFragColor = vec4(lighting, 1.0);
}}
";
    
    // light accumulation buffer
    WeakReference<Framebuffer> _workbuffer;

    public override void Init()
    {
        _plane = new MeshRenderer();
        _plane.Setup(MeshGenerator.GenerateGrid(new Vector2(2)));
        _shader = ShaderPreprocessor.CreateProgramFromString(ShaderSource);
        
        var spec = new FramebufferSpecification();
                spec.Attachments = new FramebufferAttachmentSpecification(
                    // using floating point colors
                    new FramebufferTextureSpecification(TextureFormat.ColorRGB16F)
                );
                spec.Width = 1;
                spec.Height = 1;
                _workbuffer = Framebuffer.Create(spec);
    }

    public override void Destroy()
    {
        _plane.Dispose();
        _shader.Dispose();
        _workbuffer.Dispose();
    }

    public override void Draw()
    {
        // init vars
        var shader = _shader.GetValue();
        var gbuffer = Pipeline.Buffer.GetValue();
        var workbuffer = _workbuffer.GetValue();
        
        // init shader
        shader.Use();
        shader.SetParameterVec3("uViewPosition", Matrix4x4Extension.SafeInvert(Pipeline.Camera.GetViewMatrix()).Translation);
        shader.SetParameterInt("uColor", DeferredPipeline.AttachmentIndexColor);
        shader.SetParameterInt("uPosition", DeferredPipeline.AttachmentIndexPosition);
        shader.SetParameterInt("uNormal", DeferredPipeline.AttachmentIndexNormal);
        gbuffer.BindColorAttachment(DeferredPipeline.AttachmentIndexColor, DeferredPipeline.AttachmentIndexColor);
        gbuffer.BindColorAttachment(DeferredPipeline.AttachmentIndexPosition, DeferredPipeline.AttachmentIndexPosition);
        gbuffer.BindColorAttachment(DeferredPipeline.AttachmentIndexNormal, DeferredPipeline.AttachmentIndexNormal);

        // handle ambient lighting
        bool useAmbientAccumulator = false;
        Vector3 ambientAccumulator = Vector3.Zero;
        foreach (var light in lights)
        {
            if (light is IAmbientLightRenderData ambient)
            {
                ambientAccumulator += ambient.Color;
                useAmbientAccumulator = true;
            }
        }

        ambientAccumulator = useAmbientAccumulator ? ambientAccumulator : Vector3.One / 2;
        useAmbientAccumulator = true;

        // prepare work buffer
        if (gbuffer.Size != workbuffer.Size)
            workbuffer.Resize((uint)gbuffer.Size.X, (uint)gbuffer.Size.Y);

        workbuffer.Bind();
        workbuffer.EnableColorAttachments([0]);
        workbuffer.ClearAttachment(0, Vector4.Zero);

        int pointBufferIdx = 0;
        int spotBufferIdx = 0;
        int directionalBufferIdx = 0;

        void StartBatch()
        {
            pointBufferIdx = 0;
            spotBufferIdx = 0;
            directionalBufferIdx = 0;
        }

        void NextBatch()
        {
            Flush();
            StartBatch();
        }

        void Flush()
        {
            shader.SetParameterVec3("uAmbient", useAmbientAccumulator ? ambientAccumulator : Vector3.Zero);
            useAmbientAccumulator = false;

            shader.SetParameterInt($"uNumPointLights", pointBufferIdx);
            shader.SetParameterInt($"uNumSpotLights", spotBufferIdx);
            shader.SetParameterInt($"uNumDirectionalLights", directionalBufferIdx);
            _plane.Draw(GraphicsContext.Current.Api);
        }

        void AddPointLight(IPointLightRenderData light)
        {
            shader.SetParameterVec3($"uPointLights[{pointBufferIdx}].Color", light.Color);
            shader.SetParameterVec3($"uPointLights[{pointBufferIdx}].Position", light.Position);

            const float constant = 1.0f; // note that we don't send this to the shader, we assume it is always 1.0 (in our case)
            const float linear = 0.7f;
            const float quadratic = 1.8f;
            var maxBrightness = Math.Max(Math.Max(light.Color.X, light.Color.Y), light.Color.Z);
            var radius = (-linear + MathF.Sqrt(linear * linear - 4 * quadratic * (constant - (256.0f / 5.0f) * maxBrightness))) / (2.0f * quadratic);
            shader.SetParameterFloat($"uPointLights[{pointBufferIdx}].Radius", radius);
            shader.SetParameterFloat($"uPointLights[{pointBufferIdx}].Linear", linear);
            shader.SetParameterFloat($"uPointLights[{pointBufferIdx}].Quadratic", quadratic);

            pointBufferIdx++;
            if (pointBufferIdx >= LightBatchBufferSize)
                NextBatch();
        }

        void AddSpotLight(ISpotLightRenderData light)
        {
            shader.SetParameterVec3($"uSpotLights[{spotBufferIdx}].Color", light.Color);
            shader.SetParameterVec3($"uSpotLights[{spotBufferIdx}].Position", light.Position);
            shader.SetParameterVec3($"uSpotLights[{spotBufferIdx}].Direction", light.Direction);
            shader.SetParameterFloat($"uSpotLights[{spotBufferIdx}].Angle", light.Angle);
            shader.SetParameterFloat($"uSpotLights[{spotBufferIdx}].Blend", light.Blend);

            const float constant = 1.0f; // note that we don't send this to the shader, we assume it is always 1.0 (in our case)
            const float linear = 0.7f;
            const float quadratic = 1.8f;
            var maxBrightness = Math.Max(Math.Max(light.Color.X, light.Color.Y), light.Color.Z);
            var radius = (-linear + MathF.Sqrt(linear * linear - 4 * quadratic * (constant - (256.0f / 5.0f) * maxBrightness))) / (2.0f * quadratic);
            shader.SetParameterFloat($"uSpotLights[{spotBufferIdx}].Radius", radius);
            shader.SetParameterFloat($"uSpotLights[{spotBufferIdx}].Linear", linear);
            shader.SetParameterFloat($"uSpotLights[{spotBufferIdx}].Quadratic", quadratic);
            
            spotBufferIdx++;
            if (spotBufferIdx >= LightBatchBufferSize)
                NextBatch();
        }

        void AddDirectionalLight(IDirectionalLightRenderData light)
        {
            shader.SetParameterVec3($"uDirectionalLights[{directionalBufferIdx}].Color", light.Color);
            shader.SetParameterVec3($"uDirectionalLights[{directionalBufferIdx}].Direction", light.Direction);

            directionalBufferIdx++;
            if (directionalBufferIdx >= LightBatchBufferSize)
                NextBatch();
        }

        for (int li = 0; li < lights.Count; li++)
        {
            switch (lights[li])
            {
                case ISpotLightRenderData spot: AddSpotLight(spot); break;
                case IDirectionalLightRenderData directional: AddDirectionalLight(directional); break;
                case IPointLightRenderData point: AddPointLight(point); break;
                default: break;
            }
        }
        Flush();

        workbuffer.BlitTo(Pipeline.Buffer, [(0, DeferredPipeline.AttachmentIndexColor)]);
        
        gbuffer.Bind();
    }
}

class FogPass : Pass
{
    private static readonly string ShaderSource = @"
#type vertex
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

void main()
{
    vTexCoord = aTexCoord;

    gl_Position = vec4(aPosition, 1.0);
}

#type fragment
#version 330 core
#include ""internal:utils.glsl""

in vec2 vTexCoord;
out vec4 oFragColor;

uniform sampler2D uPosition;
uniform vec4 uColor;
uniform float uStart;
uniform float uDensity;
uniform vec3 uViewPosition;

void main()
{
    vec3 position = texture(uPosition, vTexCoord).xyz;
    if (position == vec3(0))
        discard;

    float distance = length(uViewPosition - position);
    float fogFactor = exp(-pow(uDensity * max(distance - uStart, 0.0), 2.0));
    fogFactor = clamp(fogFactor, 0.0, 1.0);

    float alpha = uColor.a * (1.0 - fogFactor);

    oFragColor = vec4(uColor.rgb, alpha);

    // linear
    //float distance = length(uViewPosition - position);
    //oFragColor = vec4(uColor.xyz, clamp(uColor.w * map(distance, uStart, uEnd, 0, 1), 0, 1));
}
";

    public FogPass()
    {
        Depth = DepthFunc.None;
        Blend = BlendFunc.OneMinusSrcAlpha;
        DepthMask = false;
    }

    WeakReference<ShaderProgram> _shader;
    MeshRenderer _quad;

    public override void Init()
    {
        _shader = ShaderPreprocessor.CreateProgramFromString(ShaderSource);
        _quad = new MeshRenderer();
        _quad.Setup(MeshGenerator.GenerateGrid(Vector2.One * 2));
    }

    public override void Destroy()
    {
        _shader.Dispose();
        _shader = null;
        _quad.Dispose();
        _quad = null;
    }

    public override void Draw()
    {
        var fogComp = (CrossEngine.Components.FogComponent)CrossEngine.Scenes.SceneManager.Current?.World.Storage.GetArray(typeof(CrossEngine.Components.FogComponent))?.FirstOrDefault();
        if (fogComp == null) return;

        var gbuffer = Pipeline.Buffer.GetValue();
        var shader = _shader.GetValue();
        gbuffer.BindColorAttachment(2);
        shader.Use();
        shader.SetParameterFloat("uStart", fogComp.Start);
        shader.SetParameterFloat("uDensity", fogComp.Density);
        shader.SetParameterVec4("uColor", fogComp.Color);
        shader.SetParameterVec3("uViewPosition", Matrix4x4Extension.SafeInvert(Pipeline.Camera.GetViewMatrix()).Translation);
        _quad.Draw(GraphicsContext.Current.Api);
    }
}
