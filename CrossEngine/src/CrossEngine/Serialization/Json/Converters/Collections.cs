using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrossEngine.Serialization.Json.Converters
{
    class ArrayJsonConverter : MutableJsonConverter<Array>
    {
        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return Array.CreateInstance(type.GetElementType(), reader.GetProperty("$values").GetArrayLength());
        }

        public override void ReadJson(JsonElement reader, Array value, JsonSerializer serializer)
        {
            int i = 0;
            foreach (var item in reader.GetProperty("$values").EnumerateArray())
            {
                value.SetValue(serializer.Deserialize(item, value.GetType().GetElementType()), i);
                i++;
            }
        }

        public override void WriteJson(Utf8JsonWriter writer, Array value, JsonSerializer serializer)
        {
            writer.WritePropertyName("$values");
            writer.WriteStartArray();
            for (int i = 0; i < value.Length; i++)
            {
                serializer.Serialize(writer, value.GetValue(i));
            }
            writer.WriteEndArray();
        }
    }

    class ListInterfaceJsonConverter : MutableJsonConverter<IList>
    {
        public override void ReadJson(JsonElement reader, IList value, JsonSerializer serializer)
        {
            foreach (var item in reader.GetProperty("$values").EnumerateArray())
            {
                value.Add(serializer.Deserialize(item, value.GetType().GetGenericArguments()[0]));
            }
        }

        public override void WriteJson(Utf8JsonWriter writer, IList value, JsonSerializer serializer)
        {
            writer.WritePropertyName("$values");
            writer.WriteStartArray();
            for (int i = 0; i < value.Count; i++)
            {
                serializer.Serialize(writer, value[i]);
            }
            writer.WriteEndArray();
        }
    }

    class DictionaryInterfaceJsonConverter : MutableJsonConverter<IDictionary>
    {
        public override void ReadJson(JsonElement reader, IDictionary value, JsonSerializer serializer)
        {
            foreach (var item in reader.GetProperty("$values").EnumerateArray())
            {
                var key = item.GetProperty("$key");
                var val = item.GetProperty("$value");
                value.Add(
                    serializer.Deserialize(key, value.GetType().GetGenericArguments()[0]),
                    serializer.Deserialize(val, value.GetType().GetGenericArguments()[1])
                    );
            }
        }

        public override void WriteJson(Utf8JsonWriter writer, IDictionary value, JsonSerializer serializer)
        {
            writer.WritePropertyName("$values");
            writer.WriteStartArray();
            var keys = value.Keys.Cast<object>().ToArray();
            var vals = value.Values.Cast<object>().ToArray();
            for (int i = 0; i < value.Count; i++)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("$key");
                serializer.Serialize(writer, keys[i]);
                writer.WritePropertyName("$value");
                serializer.Serialize(writer, vals[i]);

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
