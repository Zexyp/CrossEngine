using System;
using System.Text.Json;
using System.Collections;
using System.Collections.Generic;

using CrossEngine.Serialization.Json.Converters;
using CrossEngine.Serialization.Json;

namespace CrossEngine.Serialization.Json
{
    // TODO: add lock
    public class JsonSerializerSettings
    {
        public IList<JsonConverter> Converters;
        public TypeResolver TypeResolver;
        public JsonWriterOptions WriterOptions;
        public object? Context = null;

        public JsonSerializerSettings()
        {
            Converters = new List<JsonConverter>();
        }

        public static JsonSerializerSettings CreateDefault()
        {
            return new JsonSerializerSettings()
            {
                Converters =
                    {
                        new EnumJsonConverter(),
                        new TypeJsonConverter(),
                        new Vector2JsonConverter(),
                        new Vector3JsonConverter(),
                        new Vector4JsonConverter(),
                        new QuaternionJsonConverter(),
                        new Matrix4x4JsonConverter(),
                        new ArrayJsonConverter(),
                        new ListInterfaceJsonConverter(),
                        new DictionaryInterfaceJsonConverter(),
                        new SerializableInterfaceJsonConverter(),
                        new DateTimeJsonConverter(),
                    },
                TypeResolver = new DefaultTypeResolver(),
                WriterOptions = default,
            };
        }
    }
}
