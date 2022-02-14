using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CrossEngine.ECS
{
    interface ISystem
    {
        public void Update();
    }

    abstract class System<T> : ISystem where T : Component
    {
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

        virtual public void Update() { }
    }
}
