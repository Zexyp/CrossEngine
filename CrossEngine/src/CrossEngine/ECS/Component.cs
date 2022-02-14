using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.ECS
{
    public abstract class Component
    {
        public Entity Entity { get; internal set; }

        public virtual void Attach() { }
        public virtual void Detach() { }

        public virtual void Update() { }
    }

    class AllowSingleComponentAttribute : Attribute
    {

    }
}
