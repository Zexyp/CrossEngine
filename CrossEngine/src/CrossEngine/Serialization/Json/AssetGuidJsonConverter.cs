using CrossEngine.Assets;
using CrossEngine.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CrossEngine.Serialization.Json
{
    internal class AssetGuidJsonConverter : ElementJsonConverter<Asset>
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOf(typeof(Asset));

        public override void Write(Utf8JsonWriter writer, Asset value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value.Id, options);
        }

        public override Asset Read(JsonElement reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return AssetManager.Get(typeToConvert, reader.GetGuid());
        }
    }
}
