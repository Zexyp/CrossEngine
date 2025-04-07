using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components;

public class SkyboxRendererComponent : RendererComponent//, ISkyboxRenderData
{
    [EditorNullable]
    [Serialize]
    [EditorAsset]
    public TextureAsset PosX;
    [EditorNullable]
    [Serialize]
    [EditorAsset]
    public TextureAsset NegX;
    [EditorNullable]
    [Serialize]
    [EditorAsset]
    public TextureAsset PosY;
    [EditorNullable]
    [Serialize]
    [EditorAsset]
    public TextureAsset NegY;
    [EditorNullable]
    [Serialize]
    [EditorAsset]
    public TextureAsset PosZ;
    [EditorNullable]
    [Serialize]
    [EditorAsset]
    public TextureAsset NegZ;
}