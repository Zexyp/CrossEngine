using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CrossEngine.Serialization.Json.Converters
{
    class SerializableInterfaceJsonConverter : ReferenceHandlingCreationConverter<ISerializable>
    {
        protected override void WriteProperties(JsonWriter writer, ISerializable value, JsonSerializer serializer, JsonObjectContract contract)
        {
            writer.WritePropertyName("$type");
            var valtype = value.GetType();
            if (!valtype.IsGenericType)
                serializer.Serialize(writer,
                    valtype.FullName + ", " +
                    valtype.Assembly.GetName().Name);
            else
                serializer.Serialize(writer, valtype.AssemblyQualifiedName);
            value.OnSerialize(new SerializationInfo(writer, value, serializer, contract));
        }

        protected override void Populate(JObject obj, ISerializable value, JsonSerializer serializer)
        {
            value.OnDeserialize(new SerializationInfo(obj, value, serializer));
        }
    }
}