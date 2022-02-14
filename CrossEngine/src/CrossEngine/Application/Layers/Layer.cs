using System;

using CrossEngine.Events;

namespace CrossEngine.Layers
{
    public abstract class Layer
    {
        public Layer()
        {

        }

        public virtual void OnAttach() { }
        public virtual void OnDetach() { }
        public virtual void OnJoinedUpdate() { }
        public virtual void OnUpdate() { }
        public virtual void OnRender() { }
        public virtual void OnEvent(Event e) { }
    }
}
