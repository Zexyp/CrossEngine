using System;

using System.Text.Json;

using CrossEngine.Scenes;
using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Assets;
using CrossEngine.Utils;
using CrossEngine.Logging;
using CrossEngine.Utils.Exceptions;
using System.Collections.Generic;

namespace CrossEngine.Serialization.Json.Converters
{
    class SceneConvertorContext
    {
        public Scene scene;
        public Queue<Action> postSceneDeserializationActionsQueue = new Queue<Action>();
    }

    class SceneJsonConverter : JsonConverter<Scene>
    {
        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return ((SceneConvertorContext)serializer.Settings.Context).scene;
        }

        public override void ReadJson(JsonElement reader, Scene value, JsonSerializer serializer)
        {                    
            value.AssetRegistry = (AssetRegistry)serializer.Deserialize(reader.GetProperty("AssetRegistry"), typeof(AssetRegistry));

            foreach (var entEl in reader.GetProperty("Entities").GetProperty("$values").EnumerateArray())
            {
                Entity entity;
                entity = (Entity)serializer.Deserialize(entEl, typeof(Entity));
            }

            while (((SceneConvertorContext)serializer.Settings.Context).postSceneDeserializationActionsQueue.TryDequeue(out var action))
                action.Invoke();

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

    class EntityJsonConverter : JsonConverter<Entity>
    {


        public override object Create(JsonElement reader, Type type, JsonSerializer serializer)
        {
            return ((SceneConvertorContext)serializer.Settings.Context).scene.CreateEmptyEntity();
        }

        public override void ReadJson(JsonElement reader, Entity value, JsonSerializer serializer)
        {
            //value.Enabled = reader.GetProperty("Enabled").GetBoolean();

            ((SceneConvertorContext)serializer.Settings.Context).scene.SetEntityId(value, reader.ReadGuid("Id"));

            if (reader.TryReadGuid("Parent", out Guid parentId))
                ((SceneConvertorContext)serializer.Settings.Context).postSceneDeserializationActionsQueue.Enqueue(() =>
                {
                    value.Parent = ((SceneConvertorContext)serializer.Settings.Context).scene.GetEntityById(parentId);
                });

            foreach (var compEl in reader.GetProperty("Components").GetProperty("$values").EnumerateArray())
            {
                Component component = (Component)serializer.Deserialize(compEl, typeof(Component));
                if (component != null) value.AddComponent(component);
            }
        }

        public override void WriteJson(Utf8JsonWriter writer, Entity value, JsonSerializer serializer)
        {
            //writer.WriteBoolean("Enabled", value.Enabled);

            writer.WriteString("Id", value.Id);

            if (value.Parent != null)
                writer.WriteString("Parent", value.Parent.Id);

            writer.WritePropertyName("Components");
            serializer.Serialize(writer, value.Components);
        }
    }
}
