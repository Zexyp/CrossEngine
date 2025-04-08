using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel;
using CrossEngine.Serialization;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using System.Linq;
using CrossEngine.Components;

// TODO: inherit not fully implemented

namespace CrossEngine.Ecs
{
    // a very inspectable container
    public class Entity : ICloneable, ISerializable
    {
        public int Id { get; internal set; } = 0;
        public string Name;
        protected internal World World { get; internal set; }
        
        public readonly ReadOnlyCollection<Component> Components;
        public readonly ReadOnlyCollection<Entity> Children;
        public TransformComponent? Transform { get; private set; }
        
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
                    //_parent.ChildRemoved?.Invoke(this, _parent);
                }
                _parent = value;
                if (_parent != null)
                {
                    _parent._children.Add(this);
                    //_parent.ChildAdded?.Invoke(this, _parent);
                }
                ParentChanged?.Invoke(this);
            }
        }
        
        private readonly List<Component> _components = new List<Component>();
        private readonly List<Entity> _children = new List<Entity>();
        private Entity _parent;
        
        internal event Action<Entity, Component> ComponentAdded;
        internal event Action<Entity, Component> ComponentRemoved;
        internal event Action<Entity> ParentChanged;

        public Entity()
        {
            Components = _components.AsReadOnly();
            Children = _children.AsReadOnly();
        }

        #region Component Methods
        #region Add
        public T AddComponent<T>(T component) where T : Component
        {
            return (T)AddComponent((Component)component);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            return AddComponent(new T());
        }

        public Component AddComponent(Component component)
        {
            if (component is TransformComponent tc)
            {
                Debug.Assert(Transform == null);
                Transform = tc;
            }

            _components.Add(component);

            ComponentAdded?.Invoke(this, component);

            return component;
        }
        #endregion

        #region Remove
        public void RemoveComponent(Component component)
        {
            ComponentRemoved?.Invoke(this, component);

            _components.Remove(component);

            if (component is TransformComponent tc)
            {
                Debug.Assert(Transform == tc);
                Transform = null;
            }
        }

        public void RemoveComponent(Type type)
        {
            Component component = GetComponent(type);
            RemoveComponent(component);
        }

        public void RemoveComponent<T>() where T : Component
        {
            RemoveComponent(typeof(T));
        }
        #endregion

        #region Get
        public T GetComponent<T>(bool inherit = true) where T : Component
        {
            return (T)GetComponent(typeof(T), inherit);
        }

        public Component GetComponent(Type type, bool inherit = true)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (inherit ? type.IsAssignableFrom(_components[i].GetType()) : _components[i].GetType() == type)
                {
                    return _components[i];
                }
            }
            return null;
        }

        public bool TryGetComponent<T>(out T component, bool inherit = true) where T : Component
        {
            var result = TryGetComponent(typeof(T), out var foundComp, inherit);
            component = (T)foundComp;
            return result;
        }

        public bool TryGetComponent(Type type, out Component component, bool inherit = true)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (inherit ? type.IsAssignableFrom(_components[i].GetType()) : _components[i].GetType() == type)
                {
                    component = _components[i];
                    return true;
                }
            }
            component = null;
            return false;
        }
        #endregion

        public bool HasComponent<T>()
        {
            return HasComponent(typeof(T));
        }

        public bool HasComponent(Type type)
        {
            throw new NotImplementedException();
        }
        
        public void MoveComponent(Component comp, int destinationIndex)
        {
            _components.Remove(comp);
            _components.Insert(destinationIndex, comp);
        }
        #endregion
        
        #region Hierarchy Methods
        public void MoveChild(Entity child, int destinationIndex)
        {
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
            info.AddValue("Name", Name);
            info.AddValue("Components", _components.ToArray());
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            Debug.Assert(_components.Count == 0);

            Id = info.GetValue("Id", Id);
            Name = info.GetValue("Name", Name);

            var comps = info.GetValue<Component[]>("Components");
            for (int i = 0; i < comps.Length; i++)
            {
                AddComponent(comps[i]);
            }
        }
        #endregion
        
        public object Clone()
        {
            Entity entity = new Entity();
            for (int i = 0; i < _components.Count; i++)
            {
                entity.AddComponent((Component)_components[i].Clone());
            }
            return entity;
        }
    }
}