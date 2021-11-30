using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CrossEngine.Serialization
{
    public interface ISerializable
    {
        public void OnSerialize(SerializationInfo info);
        public void OnDeserialize(SerializationInfo info);
    }
}

namespace CrossEngine.Serialization.Json.Converters
{
    class SerializableJsonConverter : ReferenceHandlingCreationConverter<ISerializable>
    {
        protected override void WriteProperties(JsonWriter writer, ISerializable value, JsonSerializer serializer, JsonObjectContract contract)
        {
            writer.WritePropertyName("$type");
            var valtype = value.GetType();
            if (!valtype.IsGenericType)
                serializer.Serialize(writer,
                    valtype.Namespace + "." +
                    valtype.Name + ", " +
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