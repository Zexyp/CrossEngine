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
    internal class AssetJsonConverter : SerializableJsonConverter, IInitializedConverter
    {
        internal AssetPool pool = null;

        public override bool CanConvert(Type typeToConvert) => typeToConvert.IsSubclassOf(typeof(Asset));

        public override void Write(Utf8JsonWriter writer, ISerializable value, JsonSerializerOptions options)
        {
            if (pool != null)
                JsonSerializer.Serialize(writer, ((Asset)value).Id, options);
            else
                base.Write(writer, value, options);
        }

        public override ISerializable Read(JsonElement reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (pool != null)
                return pool.Get(typeToConvert, reader.GetGuid());
            else
                return base.Read(reader, typeToConvert, options);
        }

        public void Init()
        {
            pool = null;
        }

        public void Finish()
        {
            pool = null;
        }
    }
}
