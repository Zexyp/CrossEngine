using System;
using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Rendering.Renderables;
using CrossEngine.Rendering.Textures;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components;

// not RendererComponent rn
public class SkyboxRendererComponent : RendererComponent, ISkyboxRenderData
{
    [EditorNullable]
    [SerializeInclude]
    [EditorAsset]
    public SkyboxAsset Skybox;

    WeakReference<Texture> ISkyboxRenderData.Texture => Skybox?.Texture;
}