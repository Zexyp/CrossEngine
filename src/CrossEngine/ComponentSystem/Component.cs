using System;
using System.Collections.Generic;
using System.Text;

namespace CrossEngine.ComponentSystem
{
    public abstract class Component
    {
        public Entity entity = null;

        public bool Active { get; private set; } = true;

        public virtual void OnAwake()
        {
        }

        public virtual void OnDie()
        {
        }

        public virtual void OnUpdate(float timestep)
        {
        }

        public virtual void OnRender()
        {
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        //public virtual void OnBind()
        //{
        //}
        //
        //public virtual void OnUnbind()
        //{
        //}

        public void Enable(bool enable)
        {
            if (enable) OnEnable();
            else OnDisable();

            Active = enable;
        }
    }
}
