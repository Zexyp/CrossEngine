using System.Numerics;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Ecs;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components;

public abstract class LightComponent : Component, ILightRenderData
{
    [SerializeInclude]
    [EditorColor]
    public Vector3 Color = Vector3.One;
    
    Vector3 ILightRenderData.Color => Color;
}