﻿using System;

using CrossEngine.Events;

namespace CrossEngine.Layers
{
    public abstract class Layer
    {
#if DEBUG
        public string debugName = null;
      
        public Layer(string debugName = null)
        {
            this.debugName = debugName;
        }
#endif
        public Layer()
        {

        }

        public virtual void OnAttach() { }
        public virtual void OnDetach() { }
        public virtual void OnUpdate(float timestep) { }
        public virtual void OnRender() { }
        public virtual void OnEvent(Event e) { }
    }
}
