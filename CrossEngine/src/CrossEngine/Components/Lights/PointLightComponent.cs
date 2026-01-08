using System.Numerics;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components;

public class PointLightComponent : LightComponent, IPointLightRenderData
{
    Vector3 IPointLightRenderData.Position => Entity.Transform?.WorldPosition ?? Vector3.Zero;
}