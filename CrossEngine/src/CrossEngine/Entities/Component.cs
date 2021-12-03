using System;

using CrossEngine.Events;
using CrossEngine.Serialization;
using CrossEngine.Logging;
using CrossEngine.Utils.Exceptions;

namespace CrossEngine.Entities.Components
{
    public abstract class Component
    {
        public bool Active { get; private set; } = false;
        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                if (Active)
                {
                    if (_enabled)
                        try { OnEnable(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnEnable), this.GetType().Name, ex); }
                    else
                        try { OnDisable(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnDisable), this.GetType().Name, ex); }
                }
            }
        }

        public bool Valid { get; internal set; } = false;

        public Entity Entity { get; internal set; } = null;

        public Component()
        {

        }

        internal void Activate()
        {
            Active = true;
            if (Enabled) try { OnEnable(); } catch (Exception ex) { Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnEnable), this.GetType().Name, ex); }
        }
        internal void Deactivate()
        {
            Active = false;
            if (Enabled) try { OnDisable(); } catch (Exception ex) {Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnDisable), this.GetType().Name, ex); }
        }

        protected virtual void OnEnable()
        {
        }
        protected virtual void OnDisable()
        {
        }

        public virtual void OnUpdate(float timestep)
        {
        }
        public virtual void OnRender(RenderEvent re)
        {
        }
        public virtual void OnEvent(Event e)
        {
        }

        public virtual void OnAttach()
        {
        }
        public virtual void OnDetach()
        {
        }

        public virtual void OnStart()
        { 
        }
        public virtual void OnEnd()
        {
        }

        public virtual void OnSerialize(SerializationInfo info)
        { 
        }
        public virtual void OnDeserialize(SerializationInfo info)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponentAttribute : Attribute
    {
        public readonly Type RequiredComponentType;
        public RequireComponentAttribute(Type componentType)
        {
            RequiredComponentType = componentType;
        }
    }
}
