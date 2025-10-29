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
        public static readonly JsonConverter[] BaseJsonConverters = {
            new SerializableJsonConverter(),

            new Vector2JsonConverter(),
            new Vector3JsonConverter(),
            new Vector4JsonConverter(),
            new QuaternionJsonConverter(),
            new Matrix4x4JsonConverter()
        };

        static readonly JsonSerializerOptions options;

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
        }

        public static void SerializeJson(Stream stream, object value, TypeResolver resolver = null, JsonConverter[] converters = null)
        {
            if (converters != null)
                for (int i = 0; i < converters.Length; i++)
                {
                    options.Converters.Add(converters[i]);
                }
            
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is ITypeResolveConverter resolveMe)
                    resolveMe.Resolver = resolver ?? TypeResolver.Default;
            }
            
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is IInitializedConverter initMe)
                    initMe.Init();
            }
            
            JsonSerializer.Serialize(stream, value, options);
            
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is IInitializedConverter finishMe)
                    finishMe.Finish();
            }
            
            if (converters != null)
                for (int i = 0; i < converters.Length; i++)
                {
                    options.Converters.Remove(converters[i]);
                }
        }

        public static object DeserializeJson(Stream stream, Type type, TypeResolver resolver = null, JsonConverter[] converters = null)
        {
            if (converters != null)
                for (int i = 0; i < converters.Length; i++)
                {
                    options.Converters.Add(converters[i]);
                }
            
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is ITypeResolveConverter resolveMe)
                    resolveMe.Resolver = resolver ?? TypeResolver.Default;
            }
            
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is IInitializedConverter initMe)
                    initMe.Init();
            }
            
            var value = JsonSerializer.Deserialize(stream, type, options);
            
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is IInitializedConverter finishMe)
                    finishMe.Finish();
            }
            
            if (converters != null)
                for (int i = 0; i < converters.Length; i++)
                {
                    options.Converters.Remove(converters[i]);
                }
            
            return value;
        }

        public static void UseAttributesWrite(object target, SerializationInfo info)
        {
            Type type = target.GetType();
            PropertyInfo[] props = type.GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                if (!Attribute.IsDefined(prop, typeof(SerializeIncludeAttribute)))
                    continue;
                info.AddValue(prop.Name, prop.GetValue(target));
            }
            FieldInfo[] fields = type.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!Attribute.IsDefined(field, typeof(SerializeIncludeAttribute)))
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
                if (!Attribute.IsDefined(prop, typeof(SerializeIncludeAttribute)))
                    continue;
                prop.SetValue(target, info.GetValue(prop.Name, prop.PropertyType, prop.GetValue(target)));
            }
            FieldInfo[] fields = type.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!Attribute.IsDefined(field, typeof(SerializeIncludeAttribute)))
                    continue;
                field.SetValue(target, info.GetValue(field.Name, field.FieldType, field.GetValue(target)));
            }
        }
    }
}
