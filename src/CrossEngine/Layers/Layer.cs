using System;

namespace CrossEngine.Layers
{
    public class Layer
    {
        public bool active = false;

        string debugName = null;

        public Layer(string debugName = null)
        {
            this.debugName = debugName;
        }

        public virtual void OnAttach()
        {
        }
        public virtual void OnDetach()
        {
        }
        public virtual void OnUpdate(float timestep)
        {
        }
        public virtual void OnRender()
        {
        }

        //public virtual void OnEvent(Event layerEvent)
        //{
        //}
    }

    //class Event
    //{
    //
    //}
}
