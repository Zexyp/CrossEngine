using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CrossEngine.Events;

namespace CrossEngine.Ecs
{
    public class EcsWorld
    {
        readonly List<System> _systems = new List<System>();
        readonly List<IUpdatedSystem> _updatedSystems = new List<IUpdatedSystem>();
        readonly List<IFixedUpdatedSystem> _fixedUpdatedSystems = new List<IFixedUpdatedSystem>();

        private event Func<Component, System> ComponentRegister;
        private event Func<Component, System> ComponentUnregister;

        public void RegisterSystem(System system)
        {
            Debug.Assert(!_systems.Contains(system));

            _systems.Add(system);
            
            if (system is IUpdatedSystem us)
                _updatedSystems.Add(us);
            if (system is IFixedUpdatedSystem fus)
                _fixedUpdatedSystems.Add(fus);

            system.World = this;
            system.Attach();
        }

        public void UnregisterSystem(System system)
        {
            Debug.Assert(_systems.Contains(system));

            _systems.Remove(system);

            if (system is IUpdatedSystem us)
                _updatedSystems.Remove(us);
            if (system is IFixedUpdatedSystem fus)
                _fixedUpdatedSystems.Remove(fus);

            NotifyRemoveAll(system);

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
            
            entity.ComponentAdded += OnEntityComponentAdded;
            entity.ComponentRemoved += OnEntityComponentRemoved;
        }

        public void RemoveEntity(Entity entity)
        {
            entity.ComponentAdded -= OnEntityComponentAdded;
            entity.ComponentRemoved -= OnEntityComponentRemoved;

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

        public void FixedUpdate()
        {
            for (int i = 0; i < _fixedUpdatedSystems.Count; i++)
            {
                _fixedUpdatedSystems[i].FixedUpdate();
            }
        }

        //public void Start()
        //{
        //    for (int i = 0; i < _systems.Count; i++)
        //    {
        //        _systems[i].Start();
        //    }
        //}

        //public void Stop()
        //{
        //    for (int i = 0; i < _systems.Count; i++)
        //    {
        //        _systems[i].Stop();
        //    }
        //}

        public void NotifyOn<T>(System bindTo) where T : Component
        {
            NotifyOn(typeof(T), bindTo);
        }

        // yep, this is a weird glue code
        public void NotifyOn(Type type, System bindTo)
        {
            ComponentRegister += (c) =>
            {
                if (type.IsAssignableFrom(c.GetType()))
                    bindTo.Register(c);
                return bindTo;
            };
            ComponentUnregister += (c) =>
            {
                if (type.IsAssignableFrom(c.GetType()))
                    bindTo.Unregister(c);
                return bindTo;
            };
        }

        private void NotifyRemoveAll(System boundTo)
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

        private void OnEntityComponentAdded(Entity sender, Component component)
        {
            component.Attached = true;
            ComponentRegister?.Invoke(component);
        }

        private void OnEntityComponentRemoved(Entity sender, Component component)
        {
            ComponentUnregister?.Invoke(component);
            component.Attached = false;
        }
    }
}
