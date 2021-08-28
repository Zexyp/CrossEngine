using CrossEngine.Events;
using CrossEngine.Entities.Components;

namespace CrossEngine.Rendering
{
    public class LineRenderEvent : RenderEvent
    {
        
    }

    public class SpriteRenderEvent : RenderEvent
    {
        public readonly SpriteRendererComponent.TransparencyMode TransparencyMode;

        public SpriteRenderEvent(SpriteRendererComponent.TransparencyMode transparencyMode)
        {
            TransparencyMode = transparencyMode;
        }
    }
}
