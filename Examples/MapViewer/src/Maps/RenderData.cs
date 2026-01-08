using System.Numerics;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Rendering.Materials;
using CrossEngine.Rendering.Meshes;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering.Textures;
using CrossEngine.Scenes;
using CrossEngine.Utils;

namespace Examples.Maps;

class RenderData : ISceneRenderData
{
    public IList<IObjectRenderData> Objects { get; set; }
    public IList<ILightRenderData> Lights { get; set; } = [];
    public ISkyboxRenderData Skybox { get; set; }
    public ICamera Camera { get; set; }
    public Vector4? ClearColor { get; } = VecColor.Gray;
        
    public Vector2 ViewportSize
    {
        get => throw new Exception();
        set
        {
            if (Camera is IResizableCamera rescam)
                rescam.Resize(value.X, value.Y);
        }
    }
        
    public class Spinner : ISpriteRenderData
    {
        public Matrix4x4 Transform => Matrix4x4.CreateRotationZ(rotation) * Matrix4x4.CreateScale(5);
        public bool IsVisible { get; set; } = true;
        public IVolume GetVolume() => null;
        public BlendMode Blend => BlendMode.Blend;
        public Vector4 Color => VecColor.White;
        public Texture Texture => texture;
        public bool IsEnabled => enabled;

        public Texture texture;
        public float rotation;
        public bool enabled;
    }

    public class Tile : IMeshRenderData
    {
        public Matrix4x4 Transform => Matrix4x4.Identity;
        public IVolume GetVolume() => null;
        public bool IsVisible { get; set; } = true;
        bool IObjectRenderData.IsEnabled => renderer != null;

        public MeshRenderer Renderer => renderer;
        public IMaterial Material => material;

        public MeshRenderer renderer;
        public DynamicMaterial material;
    }

    class DummyLight : IAmbientLightRenderData
    {
        public Vector3 Color => Vector3.One;
    }

    public Spinner spinner = new Spinner();
    public Tile tile = new Tile();
        
    public RenderData()
    {
        Objects = [spinner, tile];
        Lights = [new DummyLight()];
    }
}