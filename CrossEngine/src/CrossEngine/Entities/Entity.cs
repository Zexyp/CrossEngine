#define USE_TRANSFORM_CACHE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using CrossEngine.Events;
using CrossEngine.Scenes;
using CrossEngine.Utils;
using CrossEngine.Entities.Components;
using CrossEngine.Serialization.Json;
using CrossEngine.Logging;

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
                _enabled = value;
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

#if USE_TRANSFORM_CACHE
        public TransformComponent Transform { get; private set; }
#else
        public TransformComponent Transform { get => GetComponent<TransformComponent>(); }
#endif


        #region Events
        public delegate void OnComponentFunction(Entity sender, Component component);
        public event OnComponentFunction OnComponentAdded;
        public event OnComponentFunction OnComponentRemoved;

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
        #region Has
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

        public bool HasComponentOfType(Type type, bool inherit = false)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Type componentType = _components[i].GetType();
                if ((!inherit) ?
                    type == componentType :
                    componentType.IsSubclassOf(type))
                    return true;
            }
            return false;
        }
        #endregion

        #region Get
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
        #endregion

        #region Add
        public void AddComponent(Component component)
        {
#if USE_TRANSFORM_CACHE
            if (Transform == null && component.GetType() == typeof(TransformComponent)) Transform = (TransformComponent)component;
#endif

            _components.Add(component);
            component.Entity = this;

            if (Scene != null) Scene.Registry.AddComponent(component);

            ValidateAllComponents();

            if (Active)
            {
                if (component.Valid)
                {
                    component.OnAttach();
                    component.Activate();
                }
            }

            OnComponentAdded?.Invoke(this, component);
        }
        #endregion

        #region Remove
        public void RemoveComponent(Component component)
        {
#if USE_TRANSFORM_CACHE
            if (Transform == component) Transform = null;
#endif

            if (!_components.Contains(component)) throw new Exception("Entity doesn't have this component");

            if (Scene != null) Scene.Registry.RemoveComponent(component);

            if (Active)
            {
                if (component.Valid)
                {
                    component.Deactivate();
                    component.OnDetach();
                }
            }

            _components.Remove(component);

            if (Active) ValidateAllComponents();
            component.Entity = null;

            OnComponentRemoved?.Invoke(this, component);
        }

        public void RemoveComponent<T>() where T : Component
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (typeof(T) == _components[i].GetType())
                {
                    RemoveComponent(_components[i]);
                }
            }

            throw new Exception("Entity has no component of given type <" + typeof(T).Name + ">");
        }

        public bool TryRemoveComponent(Component component)
        {
            if (!_components.Contains(component)) return false;

            RemoveComponent(component);
            return true;
        }

        public bool TryRemoveComponent<T>() where T : Component
        {
            if (TryGetComponent(out T component))
            {
                RemoveComponent(component);
                return true;
            }

            return false;
        }
        #endregion

        #region Shift
        public void ShiftComponent(Component component, int index)
        {
            if (!_components.Contains(component)) throw new InvalidOperationException("Entity does not contain component!");
            if (index < 0 || index > _components.Count - 1) throw new InvalidOperationException("Invalid index!");

            Component h = _components[index];
            int newindex = _components.IndexOf(component);
            _components[index] = component;
            _components[newindex] = h;
        }
        #endregion
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
                    this.HierarchyNode.Parent = value.HierarchyNode;

                    value.OnChildAdded?.Invoke(value, this);
                }
                else
                {
                    if (Scene != null)
                        this.HierarchyNode.Parent.Parent = Scene.HierarchyRoot;
                    else
                        this.HierarchyNode.Parent = null;
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

            isAttaching = true;
            ValidateAllComponents();
            isAttaching = false;

            for (int i = 0; i < _components.Count; i++)
                if (_components[i].Valid) _components[i].OnAttach();

            if (Enabled) OnEnable();
        }
        internal void Deactivate()
        {
            Active = false;

            for (int i = 0; i < _components.Count; i++)
                if (_components[i].Valid) _components[i].OnDetach();

            if (Enabled) OnDisable();
        }

        private void OnEnable()
        {
            for (int i = 0; i < _components.Count; i++)
                if (_components[i].Valid) _components[i].Activate();
        }
        private void OnDisable()
        {
            for (int i = 0; i < _components.Count; i++)
                if (_components[i].Valid) _components[i].Deactivate();
        }

        public void OnUpdate(float timestep)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Enabled && _components[i].Valid)
                    _components[i].OnUpdate(timestep);
            }
        }

        public void OnEvent(Event e)
        {
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                if (_components[i].Enabled && _components[i].Valid)
                    _components[i].OnEvent(e);
            }
        }

        public void OnRender(RenderEvent re)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Enabled && _components[i].Valid)
                    _components[i].OnRender(re);
            }
        }

        private void ValidateAllComponents()
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Component component = _components[i];
                object[] requireAttributes = component.GetType().GetCustomAttributes(typeof(RequireComponentAttribute), false);

                bool valid = true;
                for (int rai = 0; rai < requireAttributes.Length; rai++)
                {
                    if (!(HasComponentOfType(((RequireComponentAttribute)requireAttributes[rai]).RequiredComponentType) ||
                        HasComponentOfType(((RequireComponentAttribute)requireAttributes[rai]).RequiredComponentType, true)))
                    {
                        //Log.Core.Warn($"component of type {component.GetType().Name} needs component of type {((RequireComponentAttribute)requireAttributes[rai]).RequiredComponentType.Name}");
                        valid = valid && false;
                    }
                    else
                        valid = valid && true;
                }

                if (component.Valid != valid)
                {
                    component.Valid = valid;
                }
            }
        }
    }
}
