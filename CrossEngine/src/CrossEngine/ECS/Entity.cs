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

        private List<Component> _components = new List<Component>();

        private Entity _parent;
        private readonly List<Entity> _children = new List<Entity>();
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
        public event Action<Entity> OnParentChanged;
        public event Action<Entity, Entity> OnChildAdded;
        public event Action<Entity, Entity> OnChildRemoved;

        public Entity()
        {
            Children = _children.AsReadOnly();
        }

        #region Add
        public Component AddComponent(Component component)
        {
            _components.Add(component);
            component.Entity = this;

            component.Attach();

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
    }
}
