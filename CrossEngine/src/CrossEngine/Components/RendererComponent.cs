using System.Numerics;
using CrossEngine.Ecs;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Components;

public abstract class RendererComponent : Component, IObjectRenderData
{
    Matrix4x4 IObjectRenderData.Transform => Entity.Transform?.GetWorldTransformMatrix() ?? Matrix4x4.Identity;
    int IObjectRenderData.Id => Entity.Id;
}