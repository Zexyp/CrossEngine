﻿using System;

using CrossEngine.Events;
using CrossEngine.Serialization;

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
                    if (_enabled) OnEnable();
                    else OnDisable();
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
            if (Enabled) OnEnable();
        }
        internal void Deactivate()
        {
            Active = false;
            if (Enabled) OnDisable();
        }

        public virtual void OnEnable()
        {
        }
        public virtual void OnDisable()
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
