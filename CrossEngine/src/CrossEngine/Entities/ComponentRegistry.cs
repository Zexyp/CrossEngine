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

        public void AddComponent(Component component)
        {
            Type type = component.GetType();

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

        //public ReadOnlyCollection<T> GetComponentsCollection<T>(/*bool inherit = false*/) where T : Component
        //{
        //    Type type = typeof(T);
        //    if (type == typeof(Component)) throw new InvalidOperationException();
        //
        //    if (_registryDict.ContainsKey(type))
        //        return ((List<T>)_registryDict[type]).AsReadOnly();
        //
        //    return null;
        //}

        public T? Find<T>(Predicate<T> match, bool inherit = false) where T : Component
        {
            Type type = typeof(T);
            if (type == typeof(Component)) throw new InvalidOperationException();

            if (!inherit)
            {
                if (_registryDict.ContainsKey(type)) return ((List<T>)_registryDict[type]).Find(match);
            }
            else
            {
                foreach (var pair in _registryDict)
                {
                    if (pair.Key.IsAssignableTo(type)) return ((List<T>)pair.Value).Find(match);
                }
            }

            return null;
        }

        public List<T> FindAll<T>(Predicate<T> match, bool inherit = false) where T : Component
        {
            Type type = typeof(T);
            if (type == typeof(Component)) throw new InvalidOperationException();

            if (!inherit)
            {
                if (_registryDict.ContainsKey(type)) return ((List<T>)_registryDict[type]).FindAll(match);
            }
            else
            {
                List<T> list = new List<T>();
                foreach (var pair in _registryDict)
                {
                    if (pair.Key.IsAssignableTo(type)) list.AddRange(((List<T>)pair.Value).FindAll(match));
                }
                return list;
            }

            return new List<T>();
        }

        public void GetComponents<T>(List<T> bucket, bool inherit = false) where T : Component
        {
            if (bucket == null) throw new ArgumentNullException();

            Type type = typeof(T);
            if (type == typeof(Component)) throw new InvalidOperationException();

            if (!inherit)
            {
                if (_registryDict.ContainsKey(type)) bucket.AddRange((List<T>)_registryDict[type]);
            }
            else
            {
                foreach (var pair in _registryDict)
                {
                    if (pair.Key.IsAssignableTo(type)) bucket.AddRange(pair.Value.Cast<T>());
                }
            }
        }

        public void GetComponentsGroup<Tmatch, Twith>(ICollection<Twith> withCollection, List<ComponentGroup<Tmatch, Twith>> bucket/*, bool inherit = false*/) where Tmatch : Component where Twith : Component
        {
            if (bucket == null) throw new ArgumentNullException();
            if (withCollection == null) throw new ArgumentNullException();

            if (withCollection.Count == 0) return;
            Type searchedType = typeof(Tmatch);
            if (!_registryDict.ContainsKey(searchedType)) return;

            List<Tmatch> match = new List<Tmatch>();
            GetComponents<Tmatch>(match);
            if (match.Count == 0) return;

            foreach (var with in withCollection)
            {
                if (with.Entity.TryGetComponent(out Tmatch matchComp)) bucket.Add(new ComponentGroup<Tmatch, Twith>(matchComp, with));
            }
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
