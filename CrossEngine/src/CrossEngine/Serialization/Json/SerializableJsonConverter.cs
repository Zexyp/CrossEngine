using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossEngine.Serialization.Json
{
    public class SerializableJsonConverter : ElementJsonConverter<ISerializable>
    {
        public override bool CanConvert(Type typeToConvert) => typeof(ISerializable).IsAssignableFrom(typeToConvert);

        public override void Write(Utf8JsonWriter writer, ISerializable value, JsonSerializerOptions options)
        {
            void Writing(string key, object? value)
            {
                writer.WritePropertyName(key);
                JsonSerializer.Serialize(writer, value, options);
            }

            var info = new SerializationInfo(Writing);

            writer.WriteStartObject();

            writer.WriteString("$type", value.GetType().FullName);

            OnSerializeContent(writer, value, options, info);

            value.GetObjectData(info);

            writer.WriteEndObject();
        }

        public override ISerializable Read(JsonElement reader, Type typeToConvert, JsonSerializerOptions options)
        {
            bool Reading(string key, Type type, out object? value)
            {
                value = null;
                bool has = reader.TryGetProperty(key, out var element);
                if (!has) return false;

                value = JsonSerializer.Deserialize(element, type, options);
                return true;
            }

            var info = new SerializationInfo(Reading);

            string typeString = reader.GetProperty("$type").GetString();
            Type type = Type.GetType(typeString);

            Debug.Assert(type != null);

            var serializable = (ISerializable)Activator.CreateInstance(type);

            serializable.SetObjectData(info);

            OnDeserializeContent(reader, serializable, options, info);

            return serializable;
        }

        protected virtual void OnSerializeContent(Utf8JsonWriter writer, ISerializable value, JsonSerializerOptions options, SerializationInfo info) { }
        protected virtual void OnDeserializeContent(JsonElement reader, ISerializable value, JsonSerializerOptions options, SerializationInfo info) { }
    }
}
