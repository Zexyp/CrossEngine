using System;

using System.Text.Json;

using CrossEngine.Scenes;
using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Assets;
using CrossEngine.Utils;
using CrossEngine.Logging;
using CrossEngine.Utils.Exceptions;

namespace CrossEngine.Serialization.Json.Converters
{
    abstract class ScenedJsonConverter<T> : JsonConverter<T>
    {
        public static Scene Scene = null;

        public ScenedJsonConverter(Scene scene)
        {
            Scene = scene;
        }
    }

    class SceneJsonConverter : ScenedJsonConverter<Scene>
    {
        public SceneJsonConverter(Scene scene) : base(scene) { }

        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return Scene;
        }

        public override void ReadJson(JsonElement reader, Scene value, JsonSerializer serializer)
        {                    
            value.AssetRegistry = (AssetRegistry)serializer.Deserialize(reader.GetProperty("AssetRegistry"), typeof(AssetRegistry));

            foreach (var entEl in reader.GetProperty("Entities").GetProperty("$values").EnumerateArray())
            {
                Entity entity;
                entity = (Entity)serializer.Deserialize(entEl, typeof(Entity));
            }

            //using (var reader = obj["Hierarchy"].CreateReader())
            //    serializer.Deserialize<TreeNode<Entity>>(reader);
        }

        public override void WriteJson(Utf8JsonWriter writer, Scene value, JsonSerializer serializer)
        {
            writer.WritePropertyName("AssetRegistry");
            serializer.Serialize(writer, value.AssetRegistry);

            writer.WritePropertyName("Entities");
            serializer.Serialize(writer, value.Entities);

            //writer.WritePropertyName("Hierarchy");
            //serializer.Serialize(writer, value.HierarchyRoot);
        }
    }

    class EntityJsonConverter : ScenedJsonConverter<Entity>
    {
        public EntityJsonConverter(Scene scene) : base(scene) { }

        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return Scene.CreateEmptyEntity();
        }

        public override void ReadJson(JsonElement reader, Entity value, JsonSerializer serializer)
        {
            //value.Enabled = reader.GetProperty("Enabled").GetBoolean();

            value.Parent = (Entity)serializer.Deserialize(reader.GetProperty("Parent"), typeof(Entity));

            foreach (var compEl in reader.GetProperty("Components").GetProperty("$values").EnumerateArray())
            {
                Component component = (Component)serializer.Deserialize(compEl, typeof(Component));
                if (component != null) value.AddComponent(component);
            }
        }

        public override void WriteJson(Utf8JsonWriter writer, Entity value, JsonSerializer serializer)
        {
            //writer.WriteBoolean("Enabled", value.Enabled);
            writer.WritePropertyName("Parent");
            serializer.Serialize(writer, value.Parent);
            writer.WritePropertyName("Components");
            serializer.Serialize(writer, value.Components);
        }
    }
}
