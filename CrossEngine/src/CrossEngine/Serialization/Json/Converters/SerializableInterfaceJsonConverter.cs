using System;
using System.Text.Json;

namespace CrossEngine.Serialization.Json.Converters
{
    class SerializableInterfaceJsonConverter : MutableJsonConverter<ISerializable>
    {
        public override void ReadJson(JsonElement reader, ISerializable value, JsonSerializer serializer)
        {
            value.SetObjectData(new JsonSerializationInfo(serializer, reader));
        }

        public override void WriteJson(Utf8JsonWriter writer, ISerializable value, JsonSerializer serializer)
        {
            //var valtype = value.GetType();
            //writer.WriteString("$type", valtype.FullName);
            value.GetObjectData(new JsonSerializationInfo(serializer, writer));
        }
    }
}
