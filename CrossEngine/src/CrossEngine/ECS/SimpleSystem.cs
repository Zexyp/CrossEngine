using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.Events;

namespace CrossEngine.ECS
{
    public enum SystemThreadMode
    {
        None = default,
        Sync,
        Async,
    }

    public interface ISystem
    {
        SystemThreadMode ThreadMode { get; }
        void Init();
        void Shutdown();
        void Update();
        virtual void Event(object e) { }
    }

    public interface ISystem<T> : ISystem where T : Component
    {
        void Register(T component);
        void Unregister(T component);
    }

    public abstract class SimpleSystem<T> : ISystem<T> where T : Component
    {
        public virtual SystemThreadMode ThreadMode => SystemThreadMode.Sync;

        protected readonly List<T> Components = new List<T>();

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

        public virtual void Init() { }
        public virtual void Shutdown() { }
        public virtual void Update() { }
    }

    public abstract class ParallelSystem<T> : SimpleSystem<T> where T : Component
    {
        protected ParallelOptions ParallelOptions = new ParallelOptions();

        public override void Update()
        {
            Parallel.ForEach(Components, ParallelOptions, Process);
        }

        protected abstract void Process(T component);
    }
}
