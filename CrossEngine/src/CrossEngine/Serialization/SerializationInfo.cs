using System;

namespace CrossEngine.Serialization
{
    public abstract class SerializationInfo
    {
        protected enum Operation
        {
            Read,
            Write,
        }

        protected readonly Operation operation;

        protected SerializationInfo(Operation operation)
        {
            this.operation = operation;
        }

        public abstract void AddValue(string name, object? value);
        public abstract object? GetValue(string name, Type typeOfValue);
        public abstract T GetValue<T>(string name);
        public abstract bool TryGetValue(string name, Type typeOfValue, out object value);
    }
}
