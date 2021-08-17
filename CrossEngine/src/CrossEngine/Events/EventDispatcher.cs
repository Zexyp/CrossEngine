using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossEngine.Events
{
    public class EventDispatcher
    {
        public readonly Event Event;
        public readonly Type TypeOfEvent;
        public readonly bool Exact;

        public EventDispatcher(Event e, bool exact = false)
        {
            Event = e;
            TypeOfEvent = e.GetType();
            Exact = exact;
        }

        public void Dispatch<T>(OnEventFunction<T> func) where T : Event
        {
            if (!Event.Handled &&
                Exact ? TypeOfEvent == typeof(T) : Event is T)
            {
                func?.Invoke((T)Event);
            }
        }

        public void Dispatch<T>(Action func) where T : Event
        {
            if (!Event.Handled &&
                Exact ? TypeOfEvent == typeof(T) : Event is T)
            {
                func?.Invoke();
            }
        }
    }
}
