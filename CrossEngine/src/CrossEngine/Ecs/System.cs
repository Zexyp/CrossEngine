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
        void OnUpdate();
    }

    public interface IFixedUpdatedSystem
    {
        void OnFixedUpdate();
    }

    public abstract class System
    {
        internal protected World World { get; internal set; }

        protected internal virtual void OnInit() { }
        protected internal virtual void OnShutdown() { }
        
        protected internal virtual void OnAttach() { }
        protected internal virtual void OnDetach() { }
        
        protected internal virtual void OnStart() { }
        protected internal virtual void OnStop() { }
    }

    /*
    public abstract class UnicastSystem<T> : System where T : Component
    {
        private bool inherit;

        public UnicastSystem(bool inherit = true)
        {
            this.inherit = inherit;
        }

        public override void OnAttach()
        {
            World.NotifyOn<T>(this, this.inherit);
        }

        public override void Register(Component component) => Register((T)component);
        public override void Unregister(Component component) => Unregister((T)component);

        public abstract void Register(T component);
        public abstract void Unregister(T component);
    }

    public abstract class MulticastSystem<T> : System where T : ITuple
    {
        private bool inherit;

        public MulticastSystem(bool inherit = true)
        {
            this.inherit = inherit;

            var types = typeof(T).GetGenericArguments();
            Debug.Assert(types.Distinct().Count() == types.Length && types.All(e => e.IsSubclassOf(typeof(Component))));
        }

        public override void OnAttach()
        {
            var types = typeof(T).GetGenericArguments();
            for (int i = 0; i < types.Length; i++)
            {
                World.NotifyOn(types[i], this, this.inherit);
            }
        }
    }
    */
}
