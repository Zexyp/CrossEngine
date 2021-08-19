using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using CrossEngine.Events;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Entities.Components;
using CrossEngine.Serialization.Json;

namespace CrossEngine.Entities
{
    public class Entity
    {
        //public uint ID { get; internal set; }
        public Scene Scene { get; private set; } = null;
        public int UID { get; private set; } = -1;
        private bool Active = false;
        private bool _enabled = true;

#if DEBUG
        public string debugName = "";
#endif

        private bool isAttaching = false;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                else _enabled = value;

                if (Active)
                {
                    if (_enabled) OnEnable();
                    else OnDisable();
                }
            }
        }

        private readonly List<Component> _components = new List<Component>();
        public ReadOnlyCollection<Component> Components { get => _components.AsReadOnly(); }

        internal readonly TreeNode<Entity> HierarchyNode;

        public TransformComponent Transform { get => GetComponent<TransformComponent>(); }

        #region Events
        public delegate void OnComponentFunction(Entity sender, Component component);
        public event OnComponentFunction OnComonentAdded;
        public event OnComponentFunction OnComonentRemoved;

        public event Action<Entity> OnParentSet;
        public event Action<Entity, Entity> OnChildAdded;
        public event Action<Entity, Entity> OnChildRemoved;
        #endregion

        internal Entity(Scene parentScene, int uid)
        {
            this.Scene = parentScene;
            this.UID = uid;
            HierarchyNode = new TreeNode<Entity>(this);
        }

        #region ECS
        public bool HasComponent(Component component)
        {
            return _components.Contains(component);
        }

        public bool HasComponent<T>(bool inherit = false) where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if ((!inherit) ?
                    typeof(T) == _components[i].GetType() :
                    _components[i] is T)
                    return true;
            }
            return false;
        }

        public T GetComponent<T>(bool inherit = false) where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if ((!inherit) ?
                    typeof(T) == comp.GetType() :
                    comp is T)
                {
                    if (!isAttaching)
                        return (T)comp;
                    else
                    {
                        if (!comp.Active)
                            comp.OnAttach();
                        return (T)comp;
                    }
                }
            }

            return null;
        }

        public bool TryGetComponent<T>(out T component, bool inherit = false) where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if ((!inherit) ?
                    typeof(T) == comp.GetType() :
                    comp is T)
                {
                    if (!isAttaching)
                    {
                        component = (T)comp;
                        return true;
                    }
                    else
                    {
                        if (!comp.Active)
                            comp.OnAttach();
                        component = (T)comp;
                        return true;
                    }
                }
            }
            component = null;
            return false;
        }

        public void AddComponent(Component component)
        {
            _components.Add(component);
            component.Entity = this;

            if (Scene != null) Scene.Registry.AddComponent(component);

            if (Active)
            {
                component.OnAttach();
                if (Enabled) component.Activate();
            }

            OnComonentAdded?.Invoke(this, component);
        }

        public void RemoveComponent(Component component)
        {
            if (!_components.Contains(component)) throw new Exception("Entity doesn't have this component");

            if (Scene != null) Scene.Registry.RemoveComponent(component);

            if (Active)
            {
                if (Enabled) component.Deactivate();
                component.OnDetach();
            }

            _components.Remove(component);
            component.Entity = null;

            OnComonentRemoved?.Invoke(this, component);
        }

        public void RemoveComponent<T>() where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (typeof(T) == _components[i].GetType())
                {
                    Component component = _components[i];
                    if (Scene != null) Scene.Registry.RemoveComponent(component);

                    if (Active)
                    {
                        if (Enabled) component.Deactivate();
                        component.OnDetach();
                    }

                    component.Entity = null;
                    _components.Remove(component);

                    OnComonentRemoved?.Invoke(this, component);
                    return;
                }
            }

            throw new Exception("Entity has no component of given type <" + typeof(T).Name + ">");
        }

        public bool TryRemoveComponent(Component component)
        {
            if (!_components.Contains(component)) return false;

            if (Scene != null) Scene.Registry.RemoveComponent(component);

            if (Active)
            {
                if (Enabled) component.Deactivate();
                component.OnDetach();
            }

            _components.Remove(component);
            component.Entity = null;

            OnComonentRemoved?.Invoke(this, component);

            return true;
        }

        public bool TryRemoveComponent<T>() where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (typeof(T) == _components[i].GetType())
                {
                    Component component = _components[i];
                    if (Scene != null) Scene.Registry.RemoveComponent(component);

                    if (Active)
                    {
                        if (Enabled) component.Deactivate();
                        component.OnDetach();
                    }

                    _components.Remove(component);
                    component.Entity = null;

                    OnComonentRemoved?.Invoke(this, component);

                    return true;
                }
            }

            return false;
        }

        public void ClearComponents()
        {
            while (_components.Count > 0) RemoveComponent(_components[0]);
        }
        #endregion

        #region Hierarchy
        public Entity Parent
        {
            get => this.HierarchyNode.Parent.Value;
            set
            {
                if (Parent == value)
                    return;

                if (Parent != null)
                {
                    Parent.OnChildRemoved?.Invoke(Parent, this);
                }

                if (value != null)
                {
                    this.HierarchyNode.SetParent(value.HierarchyNode);

                    value.OnChildAdded?.Invoke(value, this);
                }
                else
                {
                    if (Scene != null)
                        this.HierarchyNode.Parent.SetParent(Scene.HierarchyRoot);
                    else
                        this.HierarchyNode.SetParent(null);
                }

                OnParentSet?.Invoke(this);
            }
        }

        public Entity[] GetChildren()
        {
            TreeNode<Entity>[] collection = new TreeNode<Entity>[HierarchyNode.Children.Count];
            HierarchyNode.Children.CopyTo(collection, 0);
            return Array.ConvertAll(collection, (i) => { return i.Value; });
        }
        #endregion

        internal void Activate()
        {
            Active = true;
            if (Enabled) OnEnable();
        }
        internal void Deactivate()
        {
            Active = false;
            if (Enabled) OnDisable();
        }

        void OnEnable()
        {
            if (Enabled) for (int i = 0; i < _components.Count; i++) _components[i].Activate();
        }
        void OnDisable()
        {
            if (Enabled) for (int i = 0; i < _components.Count; i++) _components[i].Deactivate();
        }

        public void OnAwake()
        {
            isAttaching = true;
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].OnAttach();
            }
            isAttaching = false;
        }
        public void OnDie()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                _components[i].OnDetach();
            }
        }

        public void OnUpdate(float timestep)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Enabled)
                    _components[i].OnUpdate(timestep);
            }
        }

        public void OnEvent(Event e)
        {
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                if (_components[i].Enabled)
                    _components[i].OnEvent(e);
            }
        }

        public void OnRender(RenderEvent re)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Enabled)
                    _components[i].OnRender(re);
            }
        }
    }
}
