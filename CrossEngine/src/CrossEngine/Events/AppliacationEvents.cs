using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Events
{
    public class FixedUpdateEvent : Event
    {
        public override bool Handled { get => false; set { if (value) throw new InvalidOperationException($"{nameof(FixedUpdateEvent)} cannot be handled"); } }
    }



    public abstract class ApplicationEvent : Event { }



    public class ApplicationUpdateEvent : ApplicationEvent
    {
        public readonly float Timestep;

        public ApplicationUpdateEvent(float ts)
        {
            Timestep = ts;
        }
    }

    public class ApplicationDestroyEvent : ApplicationEvent { }
    public class ApplicationInitEvent : ApplicationEvent { }

    public class ApplicationRenderEvent : ApplicationEvent
    {

    }

    public class ApplicationRenderDestroyEvent : ApplicationEvent { }
    public class ApplicationRenderInitEvent : ApplicationEvent { }
}
