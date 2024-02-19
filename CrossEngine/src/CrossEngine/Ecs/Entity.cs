#define TRANSFORM_COMPONENT_CACHE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using CrossEngine.Serialization;
using CrossEngine.Utils;

#if TRANSFORM_COMPONENT_CACHE
using CrossEngine.Components;
#endif

namespace CrossEngine.Ecs
{
    // a very inspectable container
    public class Entity : ISerializable, ICloneable
    {
        public Guid Id { get; internal set; } = Guid.Empty;

        public readonly ReadOnlyCollection<Entity> Children;
        public readonly ReadOnlyCollection<Component> Components;

        // hierarchy events
        public event Action<Entity> ParentChanged;
        public event Action<Entity, Entity> ChildAdded;
        public event Action<Entity, Entity> ChildRemoved;

        // component events
        public event Action<Entity, Component> ComponentAdded;
        public event Action<Entity, Component> ComponentRemoved;

        public Entity Parent
        {
            get => _parent;
            set
            {
                if (value == _parent) return;

                if (this == value) throw new InvalidOperationException("Unacceptable!!! (yeah someone tried self parenting)");
                if (value?.IsParentedBy(this) ?? false) throw new InvalidOperationException("Recursive tree attempted");
                
                if (_parent != null)
                {
                    _parent._children.Remove(this);
                    _parent.ChildRemoved?.Invoke(this, _parent);
                }
                _parent = value;
                if (_parent != null)
                {
                    _parent._children.Add(this);
                    _parent.ChildAdded?.Invoke(this, _parent);
                }
                ParentChanged?.Invoke(this);
            }
        }

#if TRANSFORM_COMPONENT_CACHE
        public TransformComponent Transform { get; private set; }
#endif

        private readonly List<Component> _components = new List<Component>();
        private readonly List<Entity> _children = new List<Entity>();
        private Entity _parent = null;

        public Entity()
        {
            Components = _components.AsReadOnly();
            Children = _children.AsReadOnly();
        }

        #region Component Methods
        #region Add
        public Component AddComponent(Component component)
        {
            var componentType = component.GetType();

            if (_components.Contains(component))
                throw new InvalidOperationException("Entity already has this component");
            if (Attribute.IsDefined(componentType, typeof(AllowSinglePerEntityAttribute)))
            {
                if (GetComponent(componentType) != null)
                    throw new InvalidOperationException($"Entity already has component of type '{componentType}'");
            }

#if TRANSFORM_COMPONENT_CACHE
            if (component is TransformComponent)
            {
                Debug.Assert(Transform == null);
                Transform = (TransformComponent)component;
            }
#endif

            _components.Add(component);

            ComponentAdded?.Invoke(this, component);

            return component;
        }

        public T AddComponent<T>(T component) where T : Component
        {
            return (T)AddComponent((Component)component);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            return AddComponent(new T());
        }
        #endregion

        #region Remove
        public void RemoveComponent(Component component)
        {
            if (!_components.Contains(component))
                throw new InvalidOperationException("Entiy does not have this component");

            ComponentRemoved?.Invoke(this, component);

            _components.Remove(component);

#if TRANSFORM_COMPONENT_CACHE
            if (component is TransformComponent)
            {
                Debug.Assert(Transform == component);
                Transform = null;
            }
#endif
        }

        public void RemoveComponent<T>() where T : Component
        {
            T component = null;
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T)
                {
                    component = (T)_components[i];
                    break;
                }
            }

            RemoveComponent(component);
        }
        #endregion

        #region Get
        public T GetComponent<T>() where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T)
                {
                    return (T)_components[i];
                }
            }
            return null;
        }

        public Component GetComponent(Type type)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].GetType() == type)
                {
                    return _components[i];
                }
            }
            return null;
        }

        public T[] GetAllComponents<T>(bool inherit = true) where T : Component
        {
            List<T> found = new List<T>();
            for (int i = 0; i < _components.Count; i++)
            {
                if (inherit ? _components[i] is T : _components[i].GetType() == typeof(T))
                {
                    found.Add((T)_components[i]);
                }
            }
            return found.ToArray();
        }

        public bool TryGetComponent<T>(out T component, bool inherit = true) where T : Component
        {
            var result = TryGetComponent(typeof(T), out var output, inherit);
            component = (T)output;
            return result;
        }

        public bool TryGetComponent(Type type, out Component component, bool inherit = true)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (inherit ? type.IsAssignableFrom(_components[i].GetType()) : type == _components[i].GetType())
                {
                    component = _components[i];
                    return true;
                }
            }
            component = null;
            return false;
        }
        #endregion

        public void ShiftComponent(Component component, int destinationIndex)
        {
            if (!_components.Contains(component)) throw new InvalidOperationException("Entity does not contain component!");
            if (destinationIndex < 0 || destinationIndex > _components.Count - 1) throw new IndexOutOfRangeException("Invalid index!");

            _components.Remove(component);
            _components.Insert(destinationIndex, component);
        }

        public IEnumerable<Component> GetDeepComponents(Type type, bool inherit = true)
        {
            foreach (var c in _components)
            {
                Type comptype = c.GetType();
                if (inherit ? type.IsAssignableFrom(comptype) : comptype == type)
                    yield return c;
            }

            foreach (var ch in _children)
            {
                foreach (var c in ch.GetDeepComponents(type))
                {
                    yield return c;
                }
            }
        }
        #endregion

        #region Hierarchy Methods
        public void ShiftChild(Entity child, int destinationIndex)
        {
            if (!_children.Contains(child)) throw new InvalidOperationException("Entity does not contain child!");
            if (destinationIndex < 0 || destinationIndex > _children.Count - 1) throw new IndexOutOfRangeException("Invalid index!");

            _children.Remove(child);
            _children.Insert(destinationIndex, child);
        }

        public bool IsParentedBy(Entity potpar)
        {
            if (this.Parent == null) return false;
            if (this.Parent == potpar) return true;
            return this.Parent.IsParentedBy(potpar);
        }
        #endregion

        #region ISerializable
        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue("Id", Id);
            info.AddValue("Components", _components.ToArray());
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            Debug.Assert(_components.Count == 0);

            Id = info.GetValue<Guid>("Id");
            var comps = info.GetValue<Component[]>("Components");
            for (int i = 0; i < comps.Length; i++)
            {
                AddComponent(comps[i]);
            }
        }

        public object Clone()
        {
            Entity entity = new Entity();
            for (int i = 0; i < _components.Count; i++)
            {
                entity.AddComponent((Component)_components[i].Clone());
            }
            return entity;
        }
        #endregion
    }
}