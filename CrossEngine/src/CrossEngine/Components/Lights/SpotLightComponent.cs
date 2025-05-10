using System;
using System.Numerics;
using CrossEngine.Rendering.Lighting;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Maths;

namespace CrossEngine.Components;

public class SpotLightComponent : LightComponent, ISpotLightRenderData
{
    [SerializeInclude] [EditorDrag]
    public float Angle = 75;
    [SerializeInclude] [EditorDrag]
    public float Blend = 0;

    Vector3 IPointLightRenderData.Position => Entity.Transform?.WorldPosition ?? Vector3.Zero;
    float ISpotLightRenderData.Angle => MathExt.ToRadians(Angle);
    float ISpotLightRenderData.Blend => Blend;
    Vector3 IDirectionalLightRenderData.Direction => Vector3.Transform(-Vector3.UnitY, Entity.Transform?.WorldRotation ?? Quaternion.Identity);
}
