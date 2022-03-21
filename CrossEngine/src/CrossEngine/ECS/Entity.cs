using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.ECS
{
    public class Entity
    {
        public int Id { get; internal set; }

        private readonly List<Component> _components = new List<Component>();
        private readonly List<Entity> _children = new List<Entity>();

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

        public Entity()
        {
            Components = _components.AsReadOnly();
            Children = _children.AsReadOnly();
        }

        #region Add
        public Component AddComponent(Component component)
        {
            _components.Add(component);
            component.Entity = this;

            component.Attach();

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

            component.Detach();

            component.Entity = null;
            _components.Remove(component);
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
            if (destinationIndex < 0 || destinationIndex > _components.Count - 1) throw new InvalidOperationException("Invalid index!");

            _components.Remove(component);
            _components.Insert(destinationIndex, component);
        }

        public int GetComponentIndex(Component component) => _components.IndexOf(component);
    }
}
