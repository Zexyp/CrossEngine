using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Ecs
{
    interface IUpdatedSystem
    {
        void Update();
    }

    abstract class System
    {
        public abstract void Register(Component component);
        public abstract void Unregister(Component component);
    }

    abstract class System<T> : System where T : Component
    {
        public override void Register(Component component) => Register((T)component);
        public override void Unregister(Component component) => Unregister((T)component);

        public abstract void Register(T component);
        public abstract void Unregister(T component);
    }
}
