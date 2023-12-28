using CrossEngine.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Ecs
{
    public interface IUpdatedSystem
    {
        void Update();
    }

    public interface IFixedUpdatedSystem
    {
        void FixedUpdate();
    }

    public abstract class System
    {
        internal protected EcsWorld World { get; internal set; }

        public virtual void Attach() { }
        public virtual void Detach() { }

        //public virtual void Start() { }
        //public virtual void Stop() { }

        public abstract void Register(Component component);
        public abstract void Unregister(Component component);
    }

    public abstract class UnicastSystem<T> : System where T : Component
    {
        public override void Attach()
        {
            World.NotifyOn<T>(this);
        }

        public override void Register(Component component) => Register((T)component);
        public override void Unregister(Component component) => Unregister((T)component);

        public abstract void Register(T component);
        public abstract void Unregister(T component);
    }

    public abstract class MulticastSystem<T> : System where T : ITuple
    {
        public MulticastSystem()
        {
            var types = typeof(T).GetGenericArguments();
            if (types.Distinct().Count() == types.Length && types.All(e => e.IsSubclassOf(typeof(Component))))
                throw new NotSupportedException();
        }

        public override void Attach()
        {
            var types = typeof(T).GetGenericArguments();
            for (int i = 0; i < types.Length; i++)
            {
                World.NotifyOn(types[i], this);
            }
        }
    }
}
