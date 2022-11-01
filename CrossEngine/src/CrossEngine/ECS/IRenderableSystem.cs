using System.Collections;

using CrossEngine.Rendering;

namespace CrossEngine.ECS
{
    public interface IRenderableSystem : ISystem
    {
        (IRenderable Renderable, IList Objects) RenderData { get; }
    }
}
