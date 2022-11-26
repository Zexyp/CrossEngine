using System.Collections;

using CrossEngine.Rendering;

namespace CrossEngine.ECS
{
    public interface IRenderableComponentSystem : IComponentSystem
    {
        (IRenderable Renderable, IList Objects) RenderData { get; }
    }
}
