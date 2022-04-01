using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Events;

namespace CrossEngine.ECS
{
    enum SystemThreadMode
    {
        None = default,
        Sync,
        Async,
    }

    interface ISystem
    {
        SystemThreadMode ThreadMode { get; }
        void Init();
        void Shutdown();
        void Update();
    }

    abstract class System<T> : ISystem where T : Component
    {
        public virtual SystemThreadMode ThreadMode => SystemThreadMode.Sync;

        public static System<T> Instance { get; protected set; }

        protected readonly List<T> Components = new List<T>();

        public System()
        {
            Debug.Assert(Instance == null);

            Instance = this;
        }

        public virtual void Register(T component)
        {
            Debug.Assert(!Components.Contains(component));

            Components.Add(component);
        }

        public virtual void Unregister(T component)
        {
            Debug.Assert(Components.Contains(component));

            Components.Remove(component);
        }

        virtual public void Init() { }
        virtual public void Shutdown() { }
        virtual public void Update() { }
    }
}
