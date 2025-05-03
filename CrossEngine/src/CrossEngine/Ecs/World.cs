using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using CrossEngine.Events;
using CrossEngine.Logging;
using CrossEngine.Profiling;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CrossEngine.Ecs
{
    // TODO: multiple occurrences
    public class ComponentStorage
    {
        internal readonly ICollection<Type> Indexes;
        Dictionary<Type, List<Component>> _simpleArray = new();
        Dictionary<Type, List<Component>> _indexes = new();

        // callbacks
        Dictionary<Type, Action<Component>> _registerCallbacks = new();
        Dictionary<Type, Action<Component>> _registerCallbacksInherit = new();
        Dictionary<Type, Action<Component>> _unregisterCallbacks = new();
        Dictionary<Type, Action<Component>> _unregisterCallbacksInherit = new();

        public ComponentStorage()
        {
            Indexes = _indexes.Keys;
        }
        
        public void AddComponent(Component component)
        {
            var type = component.GetType();

            foreach (var pair in _indexes)
            {
                if (pair.Key.IsAssignableFrom(type))
                    pair.Value.Add(component);
            }

            if (!_simpleArray.ContainsKey(type))
                _simpleArray.Add(type, new());
            _simpleArray[type].Add(component);

            NotifyRegister(component);
        }

        public void RemoveComponent(Component component)
        {
            var type = component.GetType();
            foreach (var pair in _indexes)
            {
                if (pair.Key.IsAssignableFrom(type))
                    pair.Value.Remove(component);
            }

            Debug.Assert(_simpleArray.ContainsKey(type));
            _simpleArray[type].Remove(component);

            NotifyUnregister(component);
        }

        public void MakeIndex(Type type)
        {
            _indexes.Add(type, new());
        }

        public void DropIndex(Type type)
        {
            _indexes.Remove(type);
        }

        public IList<Component> GetIndex(Type type)
        {
            return _indexes[type];
        }

        public IList<Component> GetArray(Type type)
        {
            if (!_simpleArray.ContainsKey(type))
                return null;
            return _simpleArray[type];
        }

        public IEnumerable<Component> EnumerateSubclasses(Type type)
        {
            foreach (var pair in _simpleArray)
            {
                if (!type.IsAssignableFrom(pair.Key))
                    continue;
                var list = pair.Value;
                for (int i = 0; i < list.Count; i++)
                {
                    yield return list[i];
                }
            }
        }

        // this is pure ass
        public void AddNotifyRegister(Type type, Action<Component> callback, bool inherit = false)
        {
            if (!inherit)
            {
                if (!_registerCallbacks.ContainsKey(type))
                    _registerCallbacks[type] = callback;
                else
                    _registerCallbacks[type] += callback;
            }
            else
            {
                if (!_registerCallbacksInherit.ContainsKey(type))
                    _registerCallbacksInherit[type] = callback;
                else
                    _registerCallbacksInherit[type] += callback;
            }
        }

        public void AddNotifyUnregister(Type type, Action<Component> callback, bool inherit = false)
        {
            if (!inherit)
            {
                if (!_unregisterCallbacks.ContainsKey(type))
                    _unregisterCallbacks[type] = callback;
                else
                    _unregisterCallbacks[type] += callback;
            }
            else
            {
                if (!_unregisterCallbacksInherit.ContainsKey(type))
                    _unregisterCallbacksInherit[type] = callback;
                else
                    _unregisterCallbacksInherit[type] += callback;
            }
        }

        public void RemoveNotifyRegister(Type type, Action<Component> callback)
        {
            if (_registerCallbacks.ContainsKey(type))
            {
                _registerCallbacks[type] -= callback;
                if (_registerCallbacks[type] == null)
                    _registerCallbacks.Remove(type);
            }
            if (_registerCallbacksInherit.ContainsKey(type))
            {
                _registerCallbacksInherit[type] -= callback;
                if (_registerCallbacksInherit[type] == null)
                    _registerCallbacksInherit.Remove(type);
            }
        }

        public void RemoveNotifyUnregister(Type type, Action<Component> callback)
        {
            if (_unregisterCallbacks.ContainsKey(type))
            {
                _unregisterCallbacks[type] -= callback;
                if (_unregisterCallbacks[type] == null)
                    _unregisterCallbacks.Remove(type);
            }
            if (_unregisterCallbacksInherit.ContainsKey(type))
            {
                _unregisterCallbacksInherit[type] -= callback;
                if (_unregisterCallbacksInherit[type] == null)
                    _unregisterCallbacksInherit.Remove(type);
            }
        }

        private void NotifyRegister(Component component)
        {
            var type = component.GetType();
            if (_registerCallbacks.TryGetValue(type, out var action)) action.Invoke(component);
            foreach (var pair in _registerCallbacksInherit)
            {
                if (pair.Key.IsAssignableFrom(type))
                    pair.Value.Invoke(component);
            }
        }

        private void NotifyUnregister(Component component)
        {
            var type = component.GetType();
            if (_unregisterCallbacks.TryGetValue(type, out var action)) action.Invoke(component);
            foreach (var pair in _unregisterCallbacksInherit)
            {
                if (pair.Key.IsAssignableFrom(type))
                    pair.Value.Invoke(component);
            }
        }

        private int GetSubclassIndex(Type[] types, Type type)
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsAssignableFrom(type))
                    return i;
            }
            return -1;
        }

        private class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
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

        private bool _initialized = false;

        public void RegisterSystem(System system)
        {
            Debug.Assert(!_initialized);
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
            Debug.Assert(!_initialized);
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

            _initialized = true;
        }

        public void Deinit()
        {
            _initialized = false;

            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnDetach();
            }
            
            for (int i = 0; i < _systems.Count; i++)
            {
                _systems[i].OnShutdown();
            }
            
            Debug.Assert(Storage.Indexes.Count == 0);
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
