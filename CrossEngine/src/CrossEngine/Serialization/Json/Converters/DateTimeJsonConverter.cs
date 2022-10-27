using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using CrossEngine.Serialization.Json;

namespace CrossEngine.Serialization.Json.Converters
{
    class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override bool Bracketable => false;

        public override object Create(JsonElement reader, Type type, CrossEngine.Serialization.Json.JsonSerializer serializer)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void WriteJson(Utf8JsonWriter writer, DateTime value, CrossEngine.Serialization.Json.JsonSerializer serializer)
        {
            writer.WriteStringValue(value.ToString());
        }

        public override void ReadJson(JsonElement reader, DateTime value, CrossEngine.Serialization.Json.JsonSerializer serializer)
        {
        }
    }
}
