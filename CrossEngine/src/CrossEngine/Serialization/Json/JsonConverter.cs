using System;

using System.Text.Json;

namespace CrossEngine.Serialization.Json
{
    public abstract class JsonConverter
    {
        public virtual bool Bracketable => true;

        public abstract bool CanConvert(Type type);

        // TODO: mby add existing value
        public virtual object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return Activator.CreateInstance(type);
        }

        public abstract void ReadJson(JsonElement reader, object value, JsonSerializer serializer);
        public abstract void WriteJson(Utf8JsonWriter writer, object value, JsonSerializer serializer);
    }

    public abstract class JsonConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type == typeof(T);
        }

        public sealed override void ReadJson(JsonElement reader, object value, JsonSerializer serializer) => ReadJson(reader, (T)value, serializer);
        public sealed override void WriteJson(Utf8JsonWriter writer, object value, JsonSerializer serializer) => WriteJson(writer, (T)value, serializer);

        public abstract void ReadJson(JsonElement reader, T value, JsonSerializer serializer);
        public abstract void WriteJson(Utf8JsonWriter writer, T value, JsonSerializer serializer);
    }

    public abstract class MutableJsonConverter<T> : JsonConverter
    {
        public sealed override bool Bracketable => true;

        public override bool CanConvert(Type type)
        {
            return type.IsAssignableTo(typeof(T));
        }

        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            string typeString = reader.GetProperty("$type").GetString();
            Type foundType = serializer.Settings.TypeResolver.ResolveType(typeString);
            return Activator.CreateInstance(foundType);
        }

        public sealed override void ReadJson(JsonElement reader, object value, JsonSerializer serializer) => ReadJson(reader, (T)value, serializer);
        public sealed override void WriteJson(Utf8JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteString("$type", value.GetType().FullName);
            WriteJson(writer, (T)value, serializer);
        }

        public abstract void ReadJson(JsonElement reader, T value, JsonSerializer serializer);
        public abstract void WriteJson(Utf8JsonWriter writer, T value, JsonSerializer serializer);
    }
}
