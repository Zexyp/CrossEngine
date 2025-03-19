using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using CrossEngine.Serialization.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace CrossEngine.Serialization
{
    public static class Serializer
    {
        // TODO: move this somewhere
        public static readonly JsonConverter[] BaseJsonConverters = new JsonConverter[]
        {
            new SerializableJsonConverter(),

            new Vector2JsonConverter(),
            new Vector3JsonConverter(),
            new Vector4JsonConverter(),
            new QuaternionJsonConverter(),
            new Matrix4x4JsonConverter()
        };

        static readonly JsonSerializerOptions options;
        static readonly TypeResolver resolver;

        static Serializer()
        {
            options = new()
            {
#if DEBUG
                WriteIndented = true,
#endif
            };

            for (int i = 0; i < BaseJsonConverters.Length; i++)
            {
                options.Converters.Add(BaseJsonConverters[i]);
            }

            resolver = TypeResolver.Default;
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is ITypeResolveConverter resolveMe)
                    resolveMe.Resolver = resolver;
            }
        }

        public static void SerializeJson(Stream stream, object value)
        {
            JsonSerializer.Serialize(stream, value, options);
        }

        public static object DeserializeJson(Stream stream, Type type)
        {
            return JsonSerializer.Deserialize(stream, type, options);
        }

        public static void SerializeJson<T>(Stream stream, T value)
        {
            JsonSerializer.Serialize(stream, value, options);
        }

        public static T DeserializeJson<T>(Stream stream)
        {
            return JsonSerializer.Deserialize<T>(stream, options);
        }

        public static void UseAttributesWrite(object target, SerializationInfo info)
        {
            Type type = target.GetType();
            PropertyInfo[] props = type.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                if (!Attribute.IsDefined(prop, typeof(SerializeAttribute)))
                    continue;
                info.AddValue(prop.Name, prop.GetValue(target));
            }
            FieldInfo[] fields = type.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!Attribute.IsDefined(field, typeof(SerializeAttribute)))
                    continue;
                info.AddValue(field.Name, field.GetValue(target));
            }
        }

        public static void UseAttributesRead(object target, SerializationInfo info)
        {
            Type type = target.GetType();
            PropertyInfo[] props = type.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                if (!Attribute.IsDefined(prop, typeof(SerializeAttribute)))
                    continue;
                prop.SetValue(target, info.GetValue(prop.Name, prop.PropertyType));
            }
            FieldInfo[] fields = type.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!Attribute.IsDefined(field, typeof(SerializeAttribute)))
                    continue;
                field.SetValue(target, info.GetValue(field.Name, field.FieldType));
            }
        }
    }
}
