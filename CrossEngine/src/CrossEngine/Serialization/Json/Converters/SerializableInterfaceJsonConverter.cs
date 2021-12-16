using System.Text.Json;

namespace CrossEngine.Serialization.Json.Converters
{
    class SerializableInterfaceJsonConverter : MutableJsonConverter<ISerializable>
    {
        public override void ReadJson(JsonElement reader, ISerializable value, JsonSerializer serializer)
        {
            value.OnDeserialize(new JsonSerializationInfo(reader, serializer));
        }

        public override void WriteJson(Utf8JsonWriter writer, ISerializable value, JsonSerializer serializer)
        {
            var valtype = value.GetType();
            writer.WriteString("$type", valtype.FullName);
            value.OnSerialize(new JsonSerializationInfo(writer, serializer));
        }
    }
}
