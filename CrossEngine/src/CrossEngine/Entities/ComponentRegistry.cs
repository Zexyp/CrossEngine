using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using CrossEngine.Entities.Components;

namespace CrossEngine.Entities.Components
{
    public class ComponentRegistry
    {
        Dictionary<Type, IList> _registryDict = new Dictionary<Type, IList>();

        //public class ComponentAddedEvent<T> : CrossEngine.Events.Event where T : Component
        //{
        //
        //}

        public void AddComponent(Component component)
        {
            Type type = component.GetType();
            if (type == typeof(Component)) type = component.GetType();
            if (_registryDict.ContainsKey(type))
            {
                var list = _registryDict[type];
                if (!list.Contains(component))
                    list.Add(component);
                else
                    throw new InvalidOperationException("Component already added!");
            }
            else
            {
                var createdList = (IList)typeof(List<>)
                        .MakeGenericType(type)
                        .GetConstructor(new Type[0])
                        .Invoke(null);
                createdList.Add(component);

                _registryDict.Add(type, createdList);
            }
        }

        public void RemoveComponent(Component component)
        {
            Type type = component.GetType();
            if (type == typeof(Component)) type = component.GetType();
            if (_registryDict.ContainsKey(type))
            {
                var list = _registryDict[type];
                if (list.Contains(component))
                {
                    list.Remove(component);

                    // cleanup
                    if (list.Count <= 0)
                        _registryDict.Remove(type);

                    return;
                }
            }

            throw new InvalidOperationException("Component was not added!");
        }

        public ReadOnlyCollection<T> GetComponentsCollection<T>(/*, bool inherit = false*/) where T : Component
        {
            Type type = typeof(T);
            if (type == typeof(Component)) throw new InvalidOperationException();
            if (_registryDict.ContainsKey(type))
                return ((List<T>)_registryDict[type]).AsReadOnly();

            return null;
        }

        public ReadOnlyCollection<ComponentGroup<Tmatch, Twith>> GetComponentsGroup<Tmatch, Twith>(ICollection<Twith> collection/*, bool inherit = false*/) where Tmatch : Component where Twith : Component
        {
            if (collection == null) return null;
            Type searchedType = typeof(Tmatch);
            if (!_registryDict.ContainsKey(searchedType)) return null;

            List<ComponentGroup<Tmatch, Twith>> list = new List<ComponentGroup<Tmatch, Twith>>();
            var matchCol = GetComponentsCollection<Tmatch>();
            if (matchCol == null) return null;

            foreach (var with in collection)
            {
                if (with.Entity.TryGetComponent(out Tmatch match)) list.Add(new ComponentGroup<Tmatch, Twith>(match, with));
            }

            return list.AsReadOnly();
        }

        public bool ContainsType(Type type, bool inherit = false)
        {
            if (_registryDict.ContainsKey(type))
                return true;

            if (!inherit) return false;

            // inherit check
            foreach (var key in _registryDict.Keys)
            {
                if (key.IsSubclassOf(type)) return true;
            }

            return false;
        }

        public bool ContainsType<T>(bool inherit = false) where T : Component
        {
            Type type = typeof(T);
            if (type == typeof(Component)) throw new ArgumentException();
            return ContainsType(type, inherit);
        }

        public bool Contains(Component component)
        {
            Type type = component.GetType();
            return _registryDict.ContainsKey(type) && _registryDict[type].Contains(component);
        }
    }

    public class ComponentGroup<T1, T2> : Tuple<T1, T2>, IComponentGroup where T1 : Component where T2 : Component
    {
        public Entity CommonEntity { get; private init; }

        public ComponentGroup(T1 item1, T2 item2) : base(item1, item2)
        {
            if (item1.Entity == item2.Entity) CommonEntity = item1.Entity;
            else throw new InvalidOperationException($"Can't create '{nameof(ComponentGroup<T1, T2>)}' with componets of different entities.");
        }
    }

    interface IComponentGroup
    {
        public Entity CommonEntity { get; }
    }
}
