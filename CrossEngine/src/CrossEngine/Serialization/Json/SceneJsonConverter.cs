using CrossEngine.Assets;
using CrossEngine.Ecs;
using CrossEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrossEngine.Serialization.Json
{
    internal class SceneJsonConverter : ElementJsonConverter<Scene>
    {
        AssetJsonConverter assetConverter;

        public SceneJsonConverter(AssetJsonConverter assetConverter)
        {
            this.assetConverter = assetConverter;
        }

        public override void Write(Utf8JsonWriter writer, Scene value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Assets");
            JsonSerializer.Serialize(writer, value.Assets, options);
            assetConverter.pool = value.Assets;
            writer.WritePropertyName("Entities");
            JsonSerializer.Serialize(writer, value.Entities.ToArray(), options);

            writer.WriteEndObject();
        }

        public override Scene Read(JsonElement reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var scene = new Scene();

            scene.Assets = JsonSerializer.Deserialize<AssetPool>(reader.GetProperty("Assets"), options);

            assetConverter.pool = scene.Assets;

            foreach (var ent in reader.GetProperty("Entities").EnumerateArray())
            {
                scene.AddEntity(JsonSerializer.Deserialize<Entity>(ent, options));
            }

            return scene;
        }
    }
}
