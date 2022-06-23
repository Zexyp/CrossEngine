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

        public object GetValue(string name, Type typeOfValue, object customDefault)
        {
            if (TryGetValue(name, typeOfValue, out object value))
                return value;
            else
                return customDefault;
        }
        public T GetValue<T>(string name, T customDefault)
        {
            if (TryGetValue(name, out T value))
                return value;
            else
                return customDefault;
        }
        public T? GetValue<T>(string name)
        {
            return (T?)GetValue(name, typeof(T));
        }
        
        public bool TryGetValue<T>(string name, out T value)
        {
            bool succ = TryGetValue(name, typeof(T), out object objval);
            value = succ ? (T)objval : default;
            return succ;
        }
    }
}
