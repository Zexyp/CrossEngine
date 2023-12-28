using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossEngine.Events
{
    public class EventDispatcher
    {
        public readonly Event Event;
        public readonly Type TypeOfEvent;
        public bool Exact;

        public EventDispatcher(Event e, bool exact = false)
        {
            if (e == null)
                throw new ArgumentNullException();
            Event = e;
            TypeOfEvent = e.GetType();
            Exact = exact;
        }

        public EventDispatcher Dispatch<T>(OnEventFunction<T> func) where T : Event
        {
            if (!Event.Handled &&
                Exact ? TypeOfEvent == typeof(T) : Event is T)
            {
                func?.Invoke((T)Event);
            }
            return this;
        }

        public EventDispatcher Dispatch(OnEventFunction<Event> func)
        {
            if (!Event.Handled)
            {
                func?.Invoke(Event);
            }
            return this;
        }

        public EventDispatcher Dispatch(OnEventFunction<Event> func, Type eventType)
        {
            if (!Event.Handled &&
                Exact ? TypeOfEvent == eventType : eventType.IsSubclassOf(TypeOfEvent))
            {
                func?.Invoke(Event);
            }
            return this;
        }
    }
}
