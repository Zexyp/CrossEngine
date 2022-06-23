using System;
using System.Text.Json;

namespace CrossEngine.Serialization.Json
{
    public class JsonSerializationInfo : SerializationInfo
    {
        private JsonElement reader;
        private Utf8JsonWriter writer;

        private JsonSerializer serializer;

        public JsonSerializationInfo(JsonSerializer serializer, Utf8JsonWriter writer) : base(Operation.Write)
        {
            this.writer = writer;
            this.serializer = serializer;
        }

        public JsonSerializationInfo(JsonSerializer serializer, JsonElement reader) : base(Operation.Read)
        {
            this.reader = reader;
            this.serializer = serializer;
        }

        public override void AddValue(string name, object? value)
        {
            if (operation != Operation.Write) throw new InvalidOperationException();

            writer.WritePropertyName(name);
            serializer.Serialize(writer, value);
        }

        public override object? GetValue(string name, Type typeOfValue)
        {
            if (operation != Operation.Read) throw new InvalidOperationException();

            return serializer.Deserialize(reader.GetProperty(name), typeOfValue);
        }

        public override bool TryGetValue(string name, Type typeOfValue, out object value)
        {
            if (operation != Operation.Read) throw new InvalidOperationException();

            if (reader.TryGetProperty(name, out JsonElement propEl))
            {
                value = serializer.Deserialize(propEl, typeOfValue);
                return true;
            }
            value = null;
            return false;
        }

        //public bool TryGetValue<T>(string name, out T? value)
        //{
        //    if (operation != Operation.Read) throw new InvalidOperationException();
        //
        //    if (reader.TryGetProperty(name, out JsonElement propEl))
        //    {
        //        value = (T?)serializer.Deserialize(propEl, typeof(T));
        //        return true;
        //    }
        //    value = default;
        //    return false;
        //}
    }
}

