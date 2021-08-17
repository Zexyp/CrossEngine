using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using CrossEngine.Entities.Components;

namespace CrossEngine.Entities.Components
{
    class ComponentRegistry
    {
        Dictionary<Type, List<Component>> components = new Dictionary<Type, List<Component>>();

        //public class ComponentAddedEvent<T> : CrossEngine.Events.Event where T : Component
        //{
        //
        //}

        public void AddComponent(Component component)
        {
            Type type = component.GetType();
            if (components.ContainsKey(type))
            {
                if (!components[type].Contains(component))
                    components[type].Add(component);
                else
                    throw new InvalidOperationException("Component already added!");
            }
            else
                components.Add(type, new List<Component> { component });
        }

        public void RemoveComponent(Component component)
        {
            Type type = component.GetType();
            if (components.ContainsKey(type))
            {
                if (components[type].Contains(component))
                {
                    components[type].Remove(component);
                    // cleanup
                    if (components[type].Count <= 0)
                        components.Remove(type);
                    return;
                }
            }
            throw new InvalidOperationException("Component was not added!");
        }

        public ReadOnlyCollection<Component> GetComponents<T>() where T : Component
        {
            Type type = typeof(T);
            if (components.ContainsKey(type))
                return components[type].AsReadOnly();
            return null;
            // idk i'm getting lazy
            //throw new Exception("There is no component of given type");
        }

        public bool ContainsType<T>() where T : Component
        {
            return components.ContainsKey(typeof(T));
        }
    }
}
