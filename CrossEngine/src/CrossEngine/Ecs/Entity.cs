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
    public class Entity : ISerializable
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
                if (_parent != null)
                {
                    _parent._children.Remove(this);
                    _parent.ChildRemoved?.Invoke(_parent, this);
                }
                _parent = value;
                if (_parent != null)
                {
                    _parent._children.Add(this);
                    _parent.ChildAdded?.Invoke(_parent, this);
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
        private bool _attached = false;

        public Entity()
        {
            Components = _components.AsReadOnly();
            Children = _children.AsReadOnly();
        }

        internal void Attach()
        {
            _attached = true;
            for (int i = 0; i < _components.Count; i++)
            {
                AttachComponent(_components[i]);
            }
        }

        internal void Detach()
        {
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                DetachComponent(_components[i]);
            }
            _attached = false;
        }

        #region Component Methods
        #region Add
        public Component AddComponent(Component component)
        {
            Debug.Assert(!_components.Contains(component));

            var componentType = component.GetType();
            if (Attribute.GetCustomAttribute(componentType, typeof(AllowSinglePerEntityAttribute)) != null)
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

            AttachComponent(component);

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
            Debug.Assert(_components.Contains(component));

            DetachComponent(component);

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

        public T[] GetAllComponents<T>() where T : Component
        {
            List<T> found = new List<T>();
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T)
                {
                    found.Add((T)_components[i]);
                }
            }
            return found.ToArray();
        }

        public bool TryGetComponent<T>(out T component) where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T)
                {
                    component = (T)_components[i];
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

        public int GetComponentIndex(Component component) => _components.IndexOf(component);

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

        public int GetChildIndex(Entity child) => _children.IndexOf(child);

        public bool IsParentedBy(Entity potpar)
        {
            if (this.Parent == null) return false;
            if (this.Parent == potpar) return true;
            return this.Parent.IsParentedBy(potpar);
        }
        #endregion

        private void AttachComponent(Component component)
        {
            component.Entity = this;
            component.OnAttach();
            if (component.Enabled) component.OnEnable();

            ComponentAdded?.Invoke(this, component);
        }

        private void DetachComponent(Component component)
        {
            if (component.Enabled) component.OnDisable();
            component.OnDetach();
            component.Entity = null;

            ComponentRemoved?.Invoke(this, component);
        }

        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue("Id", Id);
            info.AddValue("Components", _components.ToArray());
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            Id = info.GetValue("Id", Id);
            foreach (var comp in info.GetValue<Component[]>("Components"))
            {
                AddComponent(comp);
            }
        }
    }
}