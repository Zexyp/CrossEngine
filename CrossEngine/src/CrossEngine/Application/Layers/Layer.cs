using System;

using CrossEngine.Events;

namespace CrossEngine.Layers
{
    public abstract class Layer
    {
        protected internal virtual void Attach() { }
        protected internal virtual void Detach() { }
        protected internal virtual void Update() { }
        protected internal virtual void Render() { }
        protected internal virtual void RenderAttach() { }
        protected internal virtual void RenderDetach() { }
        protected internal virtual void Event(Event e) { }
    }
}
