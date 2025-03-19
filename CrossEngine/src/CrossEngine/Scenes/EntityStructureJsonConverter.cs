using CrossEngine.Ecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CrossEngine.Serialization.Json
{
    internal class EntityStructureJsonConverter : SerializableJsonConverter, IInitializedConverter
    {
        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(Entity);

        Dictionary<int, Entity> _pool = new();
        List<(Entity Entity, int Id)> _bindToParent = new();

        protected override void OnSerializeContent(Utf8JsonWriter writer, ISerializable value, JsonSerializerOptions options, SerializationInfo info)
        {
            var entity = (Entity)value;
            if (entity.Parent != null)
                writer.WriteNumber("Parent", entity.Parent.Id);
        }

        protected override void OnDeserializeContent(JsonElement reader, ISerializable value, JsonSerializerOptions options, SerializationInfo info)
        {
            var entity = (Entity)value;
            _pool.Add(entity.Id, entity);
            if (reader.TryGetProperty("Parent", out var element))
                _bindToParent.Add((entity, element.GetInt32()));
        }

        public void Init()
        {
            _pool.Clear();
            _bindToParent.Clear();
        }

        public void Finish()
        {
            for (int i = 0; i < _bindToParent.Count; i++)
            {
                var pair = _bindToParent[i];
                pair.Entity.Parent = _pool[pair.Id];
            }
        }
    }
}
