using System;
using System.Collections.Generic;
using System.Text;

namespace CrossEngine.ComponentSystem
{
    public class Entity
    {
        List<Component> components = new List<Component> { };

        public bool Active { get; private set; } = true;

        public Transform transform = new Transform();

        #region ECS
        public T GetComponent<T>() where T : Component
        {
            foreach(Component component in components)
            {
                if (typeof(T) == component.GetType())
                    return (T)component;
            }

            return null;
        }

        public bool TryGetComponent<T>(out T outComponent) where T : Component
        {
            foreach (Component component in components)
            {
                if (typeof(T) == component.GetType())
                {
                    outComponent = (T)component;
                    return true;
                }
            }
            outComponent = null;
            return false;
        }

        public void AddComponent(Component component)
        {
            components.Add(component);
            component.entity = this;
        }

        public void RemoveComponent<T>() where T : Component
        {
            foreach(Component component in components)
            {
                if (typeof(T) == component.GetType())
                {
                    components.Remove(component);
                    return;
                }
            }
        }
        #endregion

        public void OnAwake()
        {
            foreach (Component component in components)
            {
                component.OnAwake();
            }
        }

        public void OnDie()
        {
            foreach (Component component in components)
            {
                component.OnDie();
            }
        }

        public void OnUpdate(float timestep)
        {
            foreach (Component component in components)
            {
                if (component.Active)
                    component.OnUpdate(timestep);
            }
        }

        public void OnRender()
        {
            foreach (Component component in components)
            {
                if (component.Active)
                    component.OnRender();
            }
        }

        public void Enable(bool enable)
        {
            Active = enable;
        }
    }
}
