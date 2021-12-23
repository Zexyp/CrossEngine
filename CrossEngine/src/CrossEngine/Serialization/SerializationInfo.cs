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
        public abstract bool TryGetValue(string name, Type typeOfValue, out object value);

        public object? GetValueOrDefault(string name, Type typeOfValue, object customDefault)
        {
            return GetValue(name, typeOfValue) ?? customDefault;
        }
        public T? GetValue<T>(string name)
        {
            return (T?)GetValue(name, typeof(T));
        }
        public T GetValueOrDefault<T>(string name, T customDefault = default)
        {
            return (T)GetValue(name, typeof(T)) ?? customDefault;
        }
    }
}
