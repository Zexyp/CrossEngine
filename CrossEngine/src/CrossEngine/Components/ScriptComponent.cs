using CrossEngine.Ecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Components
{
    public class ScriptComponent : Component
    {
        protected internal virtual void OnEnable() { }
        protected internal virtual void OnDisable() { }
        
        protected internal virtual void OnAttach() { }
        protected internal virtual void OnDetach() { }

        protected internal virtual void OnUpdate() { }
        protected internal virtual void OnFixedUpdate() { }
    }
}
