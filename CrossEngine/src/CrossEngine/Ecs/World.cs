using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CrossEngine.Ecs
{
    internal class World
    {
        readonly List<System> _systems = new List<System>();
        readonly List<IUpdatedSystem> _updatedSystems = new List<IUpdatedSystem>();

        private event Func<Component, System> ComponentRegister;
        private event Func<Component, System> ComponentUnregister;

        public void RegisterSystem(System system)
        {
            Debug.Assert(!_systems.Contains(system));

            _systems.Add(system);
            if (system is IUpdatedSystem updatedSystem)
                _updatedSystems.Add(updatedSystem);
            system.World = this;
            system.Attach();
        }

        public void UnregisterSystem(System system)
        {
            Debug.Assert(_systems.Contains(system));

            _systems.Remove(system);
            if (system is IUpdatedSystem updatedSystem)
                _updatedSystems.Remove(updatedSystem);
            RemoveAllNotify(system);

            system.Detach();
            system.World = null;
        }

        public T GetSystem<T>() where T : System
        {
            return (T)GetSystem(typeof(T));
        }

        public System GetSystem(Type type)
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                var system = _systems[i];
                if (system.GetType() == type)
                    return system;
            }
            return null;
        }

        public void AddEntity(Entity entity)
        {
            entity.Attach();
            for (int i = 0; i < entity.Components.Count; i++)
            {
                ComponentRegister?.Invoke(entity.Components[i]);
            }
        }

        public void RemoveEntity(Entity entity)
        {
            for (int i = entity.Components.Count - 1; i >= 0; i--)
            {
                ComponentUnregister?.Invoke(entity.Components[i]);
            }
            entity.Detach();
        }

        public void Update()
        {
            for (int i = 0; i < _updatedSystems.Count; i++)
            {
                _updatedSystems[i].Update();
            }
        }

        public void NotifyOn<T>(System bindTo) where T : Component
        {
            NotifyOn(typeof(T), bindTo);
        }

        public void NotifyOn(Type type, System bindTo)
        {
            ComponentRegister += (c) =>
            {
                if (c.GetType() == type)
                    bindTo.Register(c);
                return bindTo;
            };
            ComponentUnregister += (c) =>
            {
                if (c.GetType() == type)
                    bindTo.Unregister(c);
                return bindTo;
            };
        }

        private void RemoveAllNotify(System boundTo)
        {
            var registerInvocation = ComponentRegister.GetInvocationList();
            for (int i = 0; i < registerInvocation.Length; i++)
            {
                var del = (Func<Component, System>)registerInvocation[i];
                if (del(null) == boundTo)
                    ComponentRegister -= del;
            }
            var unregisterInvocation = ComponentUnregister.GetInvocationList();
            for (int i = 0; i < registerInvocation.Length; i++)
            {
                var del = (Func<Component, System>)unregisterInvocation[i];
                if (del(null) == boundTo)
                    ComponentUnregister -= del;
            }
        }
    }
}
