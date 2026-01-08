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
        public override void Write(Utf8JsonWriter writer, Scene value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Entities");
            JsonSerializer.Serialize(writer, value.Entities.ToArray(), options);

            writer.WriteEndObject();
        }

        public override Scene Read(JsonElement reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var scene = new Scene();

            foreach (var ent in reader.GetProperty("Entities").EnumerateArray())
            {
                scene.AddEntity(JsonSerializer.Deserialize<Entity>(ent, options));
            }

            return scene;
        }
    }
}
