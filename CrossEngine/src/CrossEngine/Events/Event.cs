using System;

using CrossEngine.Inputs;

namespace CrossEngine.Events
{
    public delegate void OnEventFunction(Event e);
    public delegate void OnEventFunction<T>(T e) where T : Event;

    abstract public class Event
    {
        public virtual bool Handled { get; set; } = false;

        public override string ToString()
        {
            Type type = this.GetType();
            return type.Name + ": {" + String.Join("; ", Array.ConvertAll(type.GetFields(), item => item.GetValue(this).ToString())) + "}";
        }
    }
}
