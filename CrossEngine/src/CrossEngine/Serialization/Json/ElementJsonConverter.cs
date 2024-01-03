using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossEngine.Serialization.Json
{
    public abstract class ElementJsonConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // https://github.com/dotnet/docs/blob/main/docs/standard/serialization/system-text-json/use-utf8jsonreader.md#related-apis 🙏
            return Read(JsonElement.ParseValue(ref reader), typeToConvert, options);
        }

        public abstract T? Read(JsonElement reader, Type typeToConvert, JsonSerializerOptions options);
    }
}
