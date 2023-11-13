﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
        internal protected World World { get; internal set; }

        public abstract void Attach();
        public abstract void Detach();

        public abstract void Register(Component component);
        public abstract void Unregister(Component component);
    }

    abstract class UnicastSystem<T> : System where T : Component
    {
        public override void Attach()
        {
            World.NotifyOn<T>(this);
        }

        public override void Detach()
        {
            
        }

        public override void Register(Component component) => Register((T)component);
        public override void Unregister(Component component) => Unregister((T)component);

        public abstract void Register(T component);
        public abstract void Unregister(T component);
    }

    abstract class MulticastSystem<T> : System where T : ITuple
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

        public override void Detach()
        {

        }
    }
}
