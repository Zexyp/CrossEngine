using System.Numerics;

using CrossEngine.Scenes;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Events;

namespace CrossEngine.Rendering.Passes
{
    public abstract class RenderPassEvent : RenderEvent
    {
        
    }

    public abstract class RenderPass
    {
        abstract public void Render(SceneData data);
    }
}
