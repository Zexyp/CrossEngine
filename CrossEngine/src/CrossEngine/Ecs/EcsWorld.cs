using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CrossEngine.Events;
using CrossEngine.Logging;

namespace CrossEngine.Ecs
{
    public class EcsWorld
    {
        readonly List<ComponentSystem> _systems = new List<ComponentSystem>();
        readonly List<IUpdatedSystem> _updatedSystems = new List<IUpdatedSystem>();
        readonly List<IFixedUpdatedSystem> _fixedUpdatedSystems = new List<IFixedUpdatedSystem>();
        readonly List<Entity> _entites;

        private event Func<Component, ComponentSystem> ComponentRegister;
        private event Func<Component, ComponentSystem> ComponentUnregister;

        private static Logger Log = new Logger("ecs");

        public void RegisterSystem(ComponentSystem system)
        {
            Debug.Assert(!_systems.Contains(system));

            _systems.Add(system);
            
            if (system is IUpdatedSystem us)
                _updatedSystems.Add(us);
            if (system is IFixedUpdatedSystem fus)
                _fixedUpdatedSystems.Add(fus);

            system.World = this;
            system.OnAttach();
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

            system.OnDetach();
            system.World = null;
        }

        public void Start()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnStart();
                _systems[i].Started = true;
            }
        }

        public void Stop()
        {
            for (int i = _systems.Count - 1; i >= 0; i--)
            {
                _systems[i].Started = false;
                _systems[i].OnStop();
            }
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
            entity.ChildAdded += OnEntityChildAdded;
            entity.ChildRemoved += OnEntityChildRemoved;

            AttachEntity(entity);

            Log.Trace($"entity added '{entity.Id}'");
        }

        public void RemoveEntity(Entity entity)
        {
            DetachEntity(entity);

            entity.ChildAdded -= OnEntityChildAdded;
            entity.ChildRemoved -= OnEntityChildRemoved;
            entity.ComponentAdded -= OnEntityComponentAdded;
            entity.ComponentRemoved -= OnEntityComponentRemoved;
            
            Log.Trace($"entity removed '{entity.Id}'");
        }

        public void Update()
        {
            for (int i = 0; i < _updatedSystems.Count; i++)
            {
                _updatedSystems[i].OnUpdate();
            }
        }

        public void FixedUpdate()
        {
            for (int i = 0; i < _fixedUpdatedSystems.Count; i++)
            {
                _fixedUpdatedSystems[i].OnFixedUpdate();
            }
        }

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
            Log.Trace($"adding component '{component.GetType().FullName}'");
            component.Entity = sender;
            ComponentRegister?.Invoke(component);
        }

        private void OnEntityComponentRemoved(Entity sender, Component component)
        {
            Log.Trace($"removing component '{component.GetType().FullName}'");
            ComponentUnregister?.Invoke(component);
            component.Entity = null;
        }

        private void OnEntityChildAdded(Entity sender, Entity child)
        {
            Log.Trace("child add");
            AddEntity(child);
        }

        private void OnEntityChildRemoved(Entity sender, Entity child)
        {
            Log.Trace("child remove");
            RemoveEntity(child);
        }

        private void AttachEntity(Entity entity)
        {
            for (int i = 0; i < entity.Components.Count; i++)
            {
                var component = entity.Components[i];
                OnEntityComponentAdded(entity, component);
            }
        }

        private void DetachEntity(Entity entity)
        {
            for (int i = 0; i < entity.Components.Count; i++)
            {
                var component = entity.Components[i];
                OnEntityComponentRemoved(entity, component);
            }
        }
    }
}
