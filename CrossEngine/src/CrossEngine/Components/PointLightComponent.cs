using System.Numerics;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components;

public class PointLightComponent : LightComponent, IPointLightRenderData
{
    [SerializeInclude]
    [EditorDrag]
    public float Radius { get; set; } = 10;
    
    Vector3 IPointLightRenderData.Position => Entity.Transform?.Position ?? Vector3.Zero;
}