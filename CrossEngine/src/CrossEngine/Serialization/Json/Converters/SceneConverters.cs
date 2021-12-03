using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using CrossEngine.Scenes;
using CrossEngine.Entities;
using CrossEngine.Entities.Components;
using CrossEngine.Assets;
using CrossEngine.Utils;
using CrossEngine.Logging;
using CrossEngine.Utils.Exceptions;

namespace CrossEngine.Serialization.Json.Converters
{
    abstract class SceneReferenceHandlingCreationConverter<T> : ReferenceHandlingCreationConverter<T> where T : class
    {
        protected Scene Scene = null;

        public SceneReferenceHandlingCreationConverter(Scene scene)
        {
            Scene = scene;
        }

        protected override void Populate(JObject obj, T value, JsonSerializer serializer) => OnDeserialize(obj, value, serializer);
        protected override void WriteProperties(JsonWriter writer, T value, JsonSerializer serializer, JsonObjectContract contract) => OnSerialize(writer, value, serializer, contract);

        protected abstract void OnDeserialize(JObject obj, T value, JsonSerializer serializer);
        protected abstract void OnSerialize(JsonWriter writer, T value, JsonSerializer serializer, JsonObjectContract contract);
    }

    class SceneJsonConverter : SceneReferenceHandlingCreationConverter<Scene>
    {
        public SceneJsonConverter(Scene scene) : base(scene) { }

        protected override Scene Create(Type objectType, Scene existingValue, JsonSerializer serializer, JObject obj)
        {
            return existingValue ?? Scene;
        }

        protected override void OnDeserialize(JObject obj, Scene value, JsonSerializer serializer)
        {
            using (var reader = obj["AssetPool"].CreateReader())
                value.AssetPool = serializer.Deserialize<AssetPool>(reader);

            foreach (var ent in obj["Entities"])
            {
                Entity entity;
                using (var par = ent.CreateReader())
                    entity = serializer.Deserialize<Entity>(par);
            }

            //using (var reader = obj["Hierarchy"].CreateReader())
            //    serializer.Deserialize<TreeNode<Entity>>(reader);
        }

        protected override void OnSerialize(JsonWriter writer, Scene value, JsonSerializer serializer, JsonObjectContract contract)
        {
            writer.WritePropertyName("AssetPool");
            serializer.Serialize(writer, value.AssetPool);

            writer.WritePropertyName("Entities");
            serializer.Serialize(writer, value.Entities);

            //writer.WritePropertyName("Hierarchy");
            //serializer.Serialize(writer, value.HierarchyRoot);
        }
    }

    class EntityJsonConverter : SceneReferenceHandlingCreationConverter<Entity>
    {
        public EntityJsonConverter(Scene scene) : base(scene) { }

        protected override Entity Create(Type objectType, Entity existingValue, JsonSerializer serializer, JObject obj)
        {
            return existingValue ?? Scene.CreateEmptyEntity();
        }

        protected override void OnDeserialize(JObject obj, Entity value, JsonSerializer serializer)
        {
            foreach (var comp in obj["Components"])
            {
                using (var compreader = comp.CreateReader())
                {
                    Component component = serializer.Deserialize<Component>(compreader);
                    value.AddComponent(component);
                }
            }

            using (var par = obj["Parent"].CreateReader())
                value.Parent = (Entity)serializer.Deserialize(par);

            value.Enabled = obj["Enabled"].Value<bool>();
        }

        protected override void OnSerialize(JsonWriter writer, Entity value, JsonSerializer serializer, JsonObjectContract contract)
        {
            writer.WritePropertyName("Enabled");
            serializer.Serialize(writer, value.Enabled);
            writer.WritePropertyName("Parent");
            serializer.Serialize(writer, value.Parent);
            writer.WritePropertyName("Components");
            serializer.Serialize(writer, value.Components);
        }
    }

    class ComponentJsonConverter : ReferenceHandlingCreationConverter<Component>
    {
        protected override void WriteProperties(JsonWriter writer, Component value, JsonSerializer serializer, JsonObjectContract contract)
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

            writer.WritePropertyName("Enabled");
            serializer.Serialize(writer, value.Enabled);

            try
            {
                value.OnSerialize(new SerializationInfo(writer, value, serializer, contract));
            } catch (Exception ex)
            {
                Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnSerialize), value.GetType().Name, ex);
            }
        }

        protected override void Populate(JObject obj, Component value, JsonSerializer serializer)
        {
            value.Enabled = obj["Enabled"].Value<bool>();

            try
            {
                value.OnDeserialize(new SerializationInfo(obj, value, serializer));
            }
            catch (Exception ex)
            {
                Log.Core.Error(ExceptionMessages.ComponentInteraction, nameof(Component.OnDeserialize), value.GetType().Name, ex);
            }
        }
    }
}
