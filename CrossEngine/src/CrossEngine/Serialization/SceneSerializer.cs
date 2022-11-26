using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

using CrossEngine.Scenes;
using CrossEngine.Serialization.Json;
using CrossEngine.Serialization.Json.Converters;

namespace CrossEngine.Serialization
{
    public static class SceneSerializer
    {
        // it's only single threaded now... 😳

        //private static readonly CustomJsonConverterCollection Converters = new CustomJsonConverterCollection()
        //{
        //    new Vector2CustomJsonConverter(),
        //    new Vector3CustomJsonConverter(),
        //    new Vector4CustomJsonConverter(),
        //    new QuaternionCustomJsonConverter(),
        //    new Matrix4x4CustomJsonConverter(),
        //
        //    new IEnumerableCustomJsonConverter(),
        //    new EnumCustomJsonConverter(),
        //
        //    new SceneCustomJsonConverter(),
        //    new EntityCustomJsonConverter(),
        //    new TextureCustomJsonConverter(),
        //};

        class CrossAssemblyTypeResolver : TypeResolver
        {
            public override Type ResolveType(string typeName)
            {
                return Type.GetType(typeName);// ?? AssemblyLoader.GetType(typeName);
            }
        }

        private readonly static JsonSerializerSettings Settings = JsonSerializerSettings.CreateDefault();
        private readonly static JsonSerializer Serializer;

        static SceneSerializer()
        {
            Settings.Converters.Add(new SceneJsonConverter());
            Settings.Converters.Add(new EntityJsonConverter());
            Settings.WriterOptions.Indented = true;
            Settings.TypeResolver = new CrossAssemblyTypeResolver();
            Serializer = new JsonSerializer(Settings);
        }

        private static void SetContext(Scene scene)
        {
            if (scene != null)
                Settings.Context = new SceneConvertorContext() { scene = scene };
            else
                Settings.Context = null;
        }

        public static void SerializeJson(Scene scene, Stream stream)
        {
            SetContext(scene);
            using (System.Text.Json.Utf8JsonWriter writer = new System.Text.Json.Utf8JsonWriter(stream, Serializer.Settings.WriterOptions))
            {
                Serializer.Serialize(writer, scene);
                writer.Flush();
                stream.Flush();
            }
            SetContext(null);
        }

        public static string SerializeJsonToString(Scene scene)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                SerializeJson(scene, stream);
                stream.Flush();
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static Scene DeserializeJson(Stream stream)
        {
            Scene scene = new Scene();
            SetContext(scene);
            using (System.Text.Json.JsonDocument reader = System.Text.Json.JsonDocument.Parse(stream))
            {
                Serializer.Deserialize(reader.RootElement, typeof(Scene));
            }
            SetContext(null);
            return scene;
        }

        public static Scene DeserializeJsonFromString(string json)
        {
            Scene scene = new Scene();
            SetContext(scene);
            using (System.Text.Json.JsonDocument reader = System.Text.Json.JsonDocument.Parse(json))
            {
                Serializer.Deserialize(reader.RootElement, typeof(Scene));
            }
            SetContext(null);
            return scene;
        }
    }
}
