﻿#define TRANSFORM_CACHE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

#if TRANSFORM_CACHE
using CrossEngine.Components;
#endif

namespace CrossEngine.ECS
{
    public class Entity
    {
        public int Id { get; internal set; }

        private readonly List<Component> _components = new List<Component>();
        private readonly List<Entity> _children = new List<Entity>();

#if TRANSFORM_CACHE
        public TransformComponent Transform { get; private set; }
#endif

        private Entity _parent;
        public Entity Parent
        {
            get => _parent;
            set
            {
                if (value == _parent) return;
                if (_parent != null)
                {
                    _parent._children.Remove(this);
                    _parent.OnChildRemoved?.Invoke(_parent, this);
                }
                _parent = value;
                if (_parent != null)
                {
                    _parent._children.Add(this);
                    _parent.OnChildAdded?.Invoke(_parent, this);
                }
                OnParentChanged?.Invoke(this);
            }
        }

        public readonly ReadOnlyCollection<Entity> Children;
        public readonly ReadOnlyCollection<Component> Components;
        
        public event Action<Entity> OnParentChanged;
        public event Action<Entity, Entity> OnChildAdded;
        public event Action<Entity, Entity> OnChildRemoved;
        public event Action<Entity, Component> OnComponentAdded;
        public event Action<Entity, Component> OnComponentRemoved;

        private World _world;

        internal Entity(World world)
        {
            _world = world;
            Components = _components.AsReadOnly();
            Children = _children.AsReadOnly();
        }

        #region Component Methods
        #region Add
        public Component AddComponent(Component component)
        {
            var componentType = component.GetType();
            if (Attribute.GetCustomAttribute(componentType, typeof(AllowSinglePerEntityAttribute)) != null)
            {
                if (GetComponent(componentType) != null)
                    throw new InvalidOperationException($"Entity already has component of type '{componentType}'");
            }

#if TRANSFORM_CACHE
            if (component is TransformComponent)
            {
                Debug.Assert(Transform == null);
                Transform = (TransformComponent)component;
            }
#endif

            _components.Add(component);
            component.Entity = this;

            component.Attach(_world);

            OnComponentAdded?.Invoke(this, component);

            return component;
        }

        public T AddComponent<T>(T component) where T : Component
        {
            return (T)AddComponent((Component)component);
        }

        public T AddComponent<T>() where T : Component
        {
            return AddComponent(Activator.CreateInstance<T>());
        }
        #endregion

        #region Remove
        public void RemoveComponent(Component component)
        {
            OnComponentRemoved?.Invoke(this, component);

            component.Detach(_world);

            component.Entity = null;
            _components.Remove(component);

#if TRANSFORM_CACHE
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
    }
}
