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
        readonly List<ComponentSystem> _systems = new List<ComponentSystem>();
        readonly List<IUpdatedSystem> _updatedSystems = new List<IUpdatedSystem>();
        readonly List<IFixedUpdatedSystem> _fixedUpdatedSystems = new List<IFixedUpdatedSystem>();

        private event Func<Component, ComponentSystem> ComponentRegister;
        private event Func<Component, ComponentSystem> ComponentUnregister;

        public void RegisterSystem(ComponentSystem system)
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

        public void UnregisterSystem(ComponentSystem system)
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

        public T GetSystem<T>() where T : ComponentSystem
        {
            return (T)GetSystem(typeof(T));
        }

        public ComponentSystem GetSystem(Type type)
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
            entity.ComponentAdded += OnEntityComponentAdded;
            entity.ComponentRemoved += OnEntityComponentRemoved;
            
            entity.Attach();
        }

        public void RemoveEntity(Entity entity)
        {
            entity.Detach();

            entity.ComponentAdded -= OnEntityComponentAdded;
            entity.ComponentRemoved -= OnEntityComponentRemoved;
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

        public void NotifyOn<T>(ComponentSystem bindTo, bool inherit = true) where T : Component
        {
            NotifyOn(typeof(T), bindTo, inherit);
        }

        // yep, this is a weird glue code
        public void NotifyOn(Type type, ComponentSystem bindTo, bool inherit = true)
        {
            ComponentRegister += (c) =>
            {
                if (inherit ? type.IsAssignableFrom(c.GetType()) : type == c.GetType())
                    bindTo.Register(c);
                return bindTo;
            };
            ComponentUnregister += (c) =>
            {
                if (inherit ? type.IsAssignableFrom(c.GetType()) : type == c.GetType())
                    bindTo.Unregister(c);
                return bindTo;
            };
        }

        private void NotifyRemoveAll(ComponentSystem boundTo)
        {
            var registerInvocation = ComponentRegister.GetInvocationList();
            for (int i = 0; i < registerInvocation.Length; i++)
            {
                var del = (Func<Component, ComponentSystem>)registerInvocation[i];
                if (del(null) == boundTo)
                    ComponentRegister -= del;
            }
            var unregisterInvocation = ComponentUnregister.GetInvocationList();
            for (int i = 0; i < registerInvocation.Length; i++)
            {
                var del = (Func<Component, ComponentSystem>)unregisterInvocation[i];
                if (del(null) == boundTo)
                    ComponentUnregister -= del;
            }
        }

        private void OnEntityComponentAdded(Entity sender, Component component)
        {
            ComponentRegister?.Invoke(component);
        }

        private void OnEntityComponentRemoved(Entity sender, Component component)
        {
            ComponentUnregister?.Invoke(component);
        }
    }
}
