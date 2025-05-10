using CrossEngine.Rendering.Lighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Components;

public class DirectionalLightComponent : LightComponent, IDirectionalLightRenderData
{
    Vector3 IDirectionalLightRenderData.Direction => Vector3.Transform(-Vector3.UnitY, Entity.Transform?.WorldRotation ?? Quaternion.Identity);
}
