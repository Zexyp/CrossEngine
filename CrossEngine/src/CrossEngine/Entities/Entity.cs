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
using CrossEngine.Utils.Exceptions;

namespace CrossEngine.Entities
{
    public class Entity
    {
        //public uint ID { get; internal set; }
        public Scene Scene { get; private set; } = null;
        public int UID { get; private set; } = -1;
        private bool Active = false;
        private bool _enabled = true;

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

        public readonly ReadOnlyCollection<Component> Components;

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

        public event Action<Entity> OnParentChanged;
        public event Action<Entity, Entity> OnChildAdded;
        public event Action<Entity, Entity> OnChildRemoved;
        #endregion

        private Entity()
        {
            Components = _components.AsReadOnly();
        }

        internal Entity(Scene parentScene, int uid) : this()
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

        public bool HasComponentOfType<T>(bool inherit = false) where T : Component
        {
            if (typeof(T) == typeof(Component)) throw new InvalidOperationException();

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
                if (type == componentType || (inherit ? componentType.IsSubclassOf(type) : false))
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
        public void AddComponet(Component component)
        {
            if (component == null) throw new ArgumentNullException();
            if (_components.Contains(component)) throw new InvalidOperationException("Entity already has this component.");
            if (Scene.Registry.Contains(component) == true) throw new InvalidOperationException("Component already contained in registry.");

#if USE_TRANSFORM_CACHE
            // only sets Transform cache if it's empty
            if (Transform == null && component.GetType() == typeof(TransformComponent)) Transform = (TransformComponent)(Component)component;
#endif

            _components.Add(component);
            component.Entity = this;

            ValidateAllComponents(component);

            if (component.Valid)
            {
                try { component.OnAttach(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnAttach), component.GetType().Name, ex); }

                if (Active)
                {
                    try { component.OnStart(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnStart), component.GetType().Name, ex); }
                    component.Activate();
                }
            }

            OnComponentAdded?.Invoke(this, component);
        }

        public T AddComponent<T>(T component) where T : Component
        {
            AddComponet(component);
            return component;
        }
        #endregion

        #region Remove
        public void RemoveComponent(Component component)
        {
            if (component == null) throw new ArgumentNullException();
            if (!_components.Contains(component)) throw new InvalidOperationException("Entity doesn't have this component.");
            //if (!Scene?.Registry.Contains(component) == true) throw new Exception("Something went wrong.");

#if USE_TRANSFORM_CACHE
            if (Transform == component) Transform = null;
#endif

            if (component.Valid)
            {
                if (Active)
                {
                    component.Deactivate();
                    try { component.OnEnd(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnEnd), component.GetType().Name, ex); }
                }

                try { component.OnDetach(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnDetach), component.GetType().Name, ex); }
            }

            _components.Remove(component);
            component.Entity = null;

#if USE_TRANSFORM_CACHE
            if (component.GetType() == typeof(TransformComponent))
                Transform = GetComponent<TransformComponent>();
#endif

            ValidateAllComponents(component);

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

        public void ShiftComponent(Component component, int destinationIndex)
        {
            if (!_components.Contains(component)) throw new InvalidOperationException("Entity does not contain component!");
            if (destinationIndex < 0 || destinationIndex > _components.Count - 1) throw new InvalidOperationException("Invalid index!");

            _components.Remove(component);
            _components.Insert(destinationIndex, component);
        }

        public int GetComponentIndex(Component component)
        {
            return _components.IndexOf(component);
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
                    this.HierarchyNode.Parent = value.HierarchyNode;

                    value.OnChildAdded?.Invoke(value, this);
                }
                else
                {
                    if (Scene != null)
                        this.HierarchyNode.Parent = Scene.HierarchyRoot;
                    else
                        this.HierarchyNode.Parent = null;
                }

                OnParentChanged?.Invoke(this);
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
                if (_components[i].Valid)
                    try { _components[i].OnStart(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnStart), _components[i].GetType().Name, ex); }

            if (Enabled) OnEnable();
        }
        internal void Deactivate()
        {
            Active = false;

            if (Enabled) OnDisable();

            for (int i = 0; i < _components.Count; i++)
                if (_components[i].Valid)
                    try { _components[i].OnEnd(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnEnd), _components[i].GetType().Name, ex); }
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
                    try { _components[i].OnUpdate(timestep); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnUpdate), _components[i].GetType().Name, ex); }
            }
        }

        public void OnEvent(Event e)
        {
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                if (e.Handled) break;
                if (_components[i].Enabled && _components[i].Valid)
                    try { _components[i].OnEvent(e); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnEvent), _components[i].GetType().Name, ex); }

            }
        }

        public void OnRender(RenderEvent re)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i].Enabled && _components[i].Valid)
                    try { _components[i].OnRender(re); } catch (Exception ex) {Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnRender), _components[i].GetType().Name, ex); }
            }
        }

        private void ValidateAllComponents(Component attachExcept = null)
        {
            for (int i = 0; i < _components.Count; i++)
            {
                Component component = _components[i];

                object[] requireAttributes = component.GetType().GetCustomAttributes(typeof(RequireComponentAttribute), false);

                bool valid = true;
                for (int rai = 0; rai < requireAttributes.Length; rai++)
                {
                    if (!HasComponentOfType(((RequireComponentAttribute)requireAttributes[rai]).RequiredComponentType, true))
                    {
                        Log.Core.Warn($"component of type '{component.GetType().Name}' needs component of type '{((RequireComponentAttribute)requireAttributes[rai]).RequiredComponentType.Name}'");
                        valid = false;
                        break;
                    }
                }

                if (component.Valid != valid)
                {
                    component.Valid = valid;

                    if (component != attachExcept)
                    {
                        if (valid) try { component.OnAttach(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnAttach), component.GetType().Name, ex); }
                    
                        if (Active)
                        {
                            if (valid)
                            {
                                if (!component.Active)
                                {
                                    try { component.OnStart(); } catch (Exception ex) {Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnStart), component.GetType().Name, ex); }
                                    component.Activate();
                                }
                            }
                            else
                            {
                                if (component.Active)
                                {
                                    component.Deactivate();
                                    try { component.OnEnd(); } catch (Exception ex) {Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnEnd), component.GetType().Name, ex); }
                                }
                            }
                        }

                        if (!valid) try { component.OnDetach(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnDetach), component.GetType().Name, ex); }
                    }
                }
            }
        }
    }
}
