using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CrossEngine.Serialization
{
    public class SerializationInfo
    {
        enum Operation
        {
            Read,
            Write,
        }

        private JObject obj;
        private JsonWriter writer;
        private JsonObjectContract contract;

        private object value;

        private JsonSerializer serializer;

        private Operation operation;

        private SerializationInfo(Operation operation)
        {
            this.operation = operation;
        }

        public SerializationInfo(JsonWriter writer, object value, JsonSerializer serializer, JsonObjectContract contract) : this(Operation.Write)
        {
            this.writer = writer;
            this.value = value;
            this.serializer = serializer;
            this.contract = contract;
        }

        public SerializationInfo(JObject obj, object value, JsonSerializer serializer) : this(Operation.Read)
        {
            this.obj = obj;
            this.value = value;
            this.serializer = serializer;
        }

        public void AddValue(string name, object? value)
        {
            if (operation != Operation.Write) throw new InvalidOperationException();

            writer.WritePropertyName(name);
            serializer.Serialize(writer, value);
        }

        public object? GetValue(string name, Type typeOfValue)
        {
            if (operation != Operation.Read) throw new InvalidOperationException();

            using (var reader = obj[name].CreateReader())
                return serializer.Deserialize(reader, typeOfValue);
        }

        public T? GetValue<T>(string name)
        {
            if (operation != Operation.Read) throw new InvalidOperationException();

            using (var reader = obj[name].CreateReader())
                return serializer.Deserialize<T>(reader);
        }

        public bool TryGetValue(string name, Type typeOfValue, out object? value)
        {
            if (operation != Operation.Read) throw new InvalidOperationException();

            if (obj.TryGetValue(name, out JToken jt))
            {
                using (var reader = jt.CreateReader())
                    value = serializer.Deserialize(reader, typeOfValue);
                return true;
            }
            value = null;
            return false;
        }

        public bool TryGetValue<T>(string name, out T? value)
        {
            if (operation != Operation.Read) throw new InvalidOperationException();

            if (obj.TryGetValue(name, out JToken jt))
            {
                using (var reader = jt.CreateReader())
                    value = serializer.Deserialize<T>(reader);
                return true;
            }
            value = default;
            return false;
        }
    }
}
