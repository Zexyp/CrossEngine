using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.ECS;
using CrossEngine.ComponentSystems;

namespace CrossEngine.Components
{
    public abstract class ScriptableComponent : Component
    {
        public override void Attach()
        {
            ScriptableSystem.Instance.Register(this);
        }

        public override void Detach()
        {
            ScriptableSystem.Instance.Unregister(this);
        }
    }
}
