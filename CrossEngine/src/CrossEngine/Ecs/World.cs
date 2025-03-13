using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Profiling;

namespace CrossEngine.Ecs
{
    // TODO: multiple occurrences
    public class ComponentStorage
    {
        static Logger _log = new Logger("comp-store");

        Dictionary<Type[], Dictionary<Entity, Component[]>> indexes = new(new TypeArrayEqualityComparer());
        Dictionary<Type, List<Component>> _simpleArray = new();
        Dictionary<Type, Action<Component>> _registerCallbacks = new();
        Dictionary<Type, Action<Component>> _unregisterCallbacks = new();

        public void AddComponent(Component component)
        {
            _log.Trace($"add comp {component}");

            var type = component.GetType();
            foreach (var index in indexes)
            {
                if (!index.Key.Contains(type)) // skip if not needed
                    continue;
                
                bool contianed = index.Value.ContainsKey(component.Entity);
                var array = contianed ? index.Value[component.Entity] : new Component[index.Key.Length];
                
                Debug.Assert(array[Array.IndexOf(index.Key, type)] == null);
                
                array[Array.IndexOf(index.Key, type)] = component;
                if (!contianed)
                    index.Value.Add(component.Entity, array);
            }

            if (!_simpleArray.ContainsKey(type))
                _simpleArray.Add(type, new());
            _simpleArray[type].Add(component);

            if (_registerCallbacks.TryGetValue(type, out var action)) action?.Invoke(component);
        }

        public void RemoveComponent(Component component)
        {
            _log.Trace($"remove comp {component}");

            var type = component.GetType();
            foreach (var index in indexes)
            {
                if (!index.Key.Contains(type)) // skip if not needed
                    continue;
                
                if (!index.Value.ContainsKey(component.Entity)) // continue if not found
                    continue;
                
                index.Value[component.Entity][Array.IndexOf(index.Key, type)] = null;

                if (index.Value[component.Entity].All(c => c == null))
                    index.Value.Remove(component.Entity);
            }

            Debug.Assert(_simpleArray.ContainsKey(type));
            _simpleArray[type].Remove(component);

            if (_unregisterCallbacks.TryGetValue(type, out var action)) action?.Invoke(component);
        }

        public void MakeIndex(Type[] types)
        {
            indexes.Add(types, new());
        }

        public void DropIndex(Type[] types)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Component[]> IterateIndex(Type[] types)
        {
            foreach (var item in indexes[types])
            {
                yield return item.Value;
            }
        }

        public IReadOnlyList<Component> GetArray(Type type)
        {
            return _simpleArray[type];
        }

        // this is pure ass
        public void AddNotifyRegister(Type type, Action<Component> callback)
        {
            _registerCallbacks.Add(type, callback);
        }

        public void AddNotifyUnregister(Type type, Action<Component> callback)
        {
            _unregisterCallbacks.Add(type, callback);
        }

        public void RemoveNotifyRegister(Type type, Action<Component> callback)
        {
            _registerCallbacks.Remove(type);
        }

        public void RemoveNotifyUnregister(Type type, Action<Component> callback)
        {
            _unregisterCallbacks.Remove(type);
        }

        public class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
        {
            public bool Equals(Type[] x, Type[] y)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }

                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }
                
                return true;
            }

            public int GetHashCode(Type[] obj)
            {
                unchecked
                {
                    var v = 0;
                    
                    // fuck rider for wanting to convert for loops into foreach loops
                    for (int i = 0; i < obj.Length; i++)
                    {
                        v += obj[i].GetHashCode();
                    }

                    return v;
                }
            }
        }
    }
    
    public class World
    {
        readonly List<System> _systems = new List<System>();
        readonly List<IUpdatedSystem> _updatedSystems = new List<IUpdatedSystem>();
        readonly List<IFixedUpdatedSystem> _fixedUpdatedSystems = new List<IFixedUpdatedSystem>();
        readonly List<Entity> _entites;
        public readonly ComponentStorage Storage = new();

        private static Logger Log = new Logger("ecs");

        public void RegisterSystem(System system)
        {
            Debug.Assert(!_systems.Contains(system));

            _systems.Add(system);
            
            if (system is IUpdatedSystem us)
                _updatedSystems.Add(us);
            if (system is IFixedUpdatedSystem fus)
                _fixedUpdatedSystems.Add(fus);

            Debug.Assert(system.World == null);
            system.World = this;
        }

        public void UnregisterSystem(System system)
        {
            Debug.Assert(_systems.Contains(system));

            _systems.Remove(system);

            if (system is IUpdatedSystem us)
                _updatedSystems.Remove(us);
            if (system is IFixedUpdatedSystem fus)
                _fixedUpdatedSystems.Remove(fus);

            Debug.Assert(system.World == this);
            system.World = null;
        }

        public void Init()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnInit();
            }
            
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnAttach();
            }
        }

        public void Deinit()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnDetach();
            }
            
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnShutdown();
            }
        }

        public void Start()
        {
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnStart();
            }
        }

        public void Stop()
        {
            for (int i = _systems.Count - 1; i >= 0; i--)
            {
                _systems[i].OnStop();
            }
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
            Profiler.BeginScope();

            entity.ComponentAdded += OnEntityComponentAdded;
            entity.ComponentRemoved += OnEntityComponentRemoved;

            AttachEntity(entity);

            Log.Trace($"entity added '{entity.Id}'");
            
            Profiler.EndScope();
        }

        public void RemoveEntity(Entity entity)
        {
            Profiler.BeginScope();

            DetachEntity(entity);

            entity.ComponentRemoved -= OnEntityComponentRemoved;
            entity.ComponentAdded -= OnEntityComponentAdded;

            Log.Trace($"entity removed '{entity.Id}'");

            Profiler.EndScope();
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

        private void OnEntityComponentAdded(Entity sender, Component component)
        {
            Profiler.Function();

            Log.Trace($"adding component '{component.GetType().FullName}'");

            Debug.Assert(component.Entity == null);
            component.Entity = sender;
            
            Storage.AddComponent(component);
        }

        private void OnEntityComponentRemoved(Entity sender, Component component)
        {
            Profiler.Function();

            Storage.RemoveComponent(component);

            Log.Trace($"removing component '{component.GetType().FullName}'");
            
            Debug.Assert(component.Entity == sender);
            component.Entity = null;
        }

        private void AttachEntity(Entity entity)
        {
            Profiler.BeginScope();

            Debug.Assert(entity.World == null);
            entity.World = this;

            for (int i = 0; i < entity.Components.Count; i++)
            {
                var component = entity.Components[i];
                OnEntityComponentAdded(entity, component);
            }

            Profiler.EndScope();
        }

        private void DetachEntity(Entity entity)
        {
            Profiler.BeginScope();

            for (int i = 0; i < entity.Components.Count; i++)
            {
                var component = entity.Components[i];
                OnEntityComponentRemoved(entity, component);
            }

            Debug.Assert(entity.World == this);
            entity.World = null;

            Profiler.EndScope();
        }
    }
}
