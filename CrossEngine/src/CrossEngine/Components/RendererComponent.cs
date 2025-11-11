using System.Numerics;
using CrossEngine.Ecs;
using CrossEngine.Rendering.Culling;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Components;

public abstract class RendererComponent : Component, IObjectRenderData
{
    Matrix4x4 IObjectRenderData.Transform => Entity.Transform?.GetWorldTransformMatrix() ?? Matrix4x4.Identity;
    int IObjectRenderData.Id => Entity.Id;
    IVolume IObjectRenderData.GetVolume() => GetVolume();
    bool IObjectRenderData.IsVisible { get; set; } = true;
    bool IObjectRenderData.IsEnabled => Enabled;
    
    protected virtual IVolume GetVolume() { return null; }
}