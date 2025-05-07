using System.Numerics;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Rendering.Lighting;

public interface ILightRenderData
{
    Vector3 Color { get; }
}

public interface IAmbientLightRenderData : ILightRenderData;

public interface IPointLightRenderData : ILightRenderData
{
    Vector3 Position { get; }
    float Radius { get; }
}

public interface IDirectionalLightRenderData : ILightRenderData
{
    Vector3 Direction { get; }
}

public interface ISpotLightRenderData : IDirectionalLightRenderData, IPointLightRenderData
{
    float Angle { get; }
    float Blend { get; }
}