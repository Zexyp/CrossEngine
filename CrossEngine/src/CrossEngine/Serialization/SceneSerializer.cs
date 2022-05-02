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
//using CrossEngine.Assemblies;

namespace CrossEngine.Serialization
{
    public class SceneSerializer
    {
        //private static readonly CustomJsonConverterCollection Converters = new CustomJsonConverterCollection()
        //{
        //    new Vector2CustomJsonConverter(),
        //    new Vector3CustomJsonConverter(),
        //    new Vector4CustomJsonConverter(),
        //    new QuaternionCustomJsonConverter(),
        //    new Matrix4x4CustomJsonConverter(),

        //    new IEnumerableCustomJsonConverter(),
        //    new EnumCustomJsonConverter(),

        //    new SceneCustomJsonConverter(),
        //    new EntityCustomJsonConverter(),
        //    new TextureCustomJsonConverter(),
        //};

        //class CrossAssemblyTypeResolver : TypeResolver
        //{
        //    public override Type ResolveType(string typeName)
        //    {
        //        return Type.GetType(typeName) ?? AssemblyLoader.GetType(typeName);
        //    }
        //}

        private static JsonSerializerSettings CreateSettings(Scene scene)
        {
            var settings = JsonSerializerSettings.Default;
            settings.Converters.Add(new SceneJsonConverter(scene));
            settings.Converters.Add(new EntityJsonConverter(scene));
            //settings.TypeResolver = new CrossAssemblyTypeResolver();
            return settings;
        }

        public static string SertializeJson(Scene scene)
        {
            JsonSerializer serializer = new JsonSerializer(CreateSettings(scene));

            serializer.Settings.WriterOptions.Indented = true;
            
            using (MemoryStream stream = new MemoryStream())
            using (System.Text.Json.Utf8JsonWriter writer = new System.Text.Json.Utf8JsonWriter(stream, serializer.Settings.WriterOptions))
            {
                serializer.Serialize(writer, scene);
                writer.Flush();
                stream.Flush();
                return Encoding.Default.GetString(stream.ToArray());
            }
        }

        public static Scene DeserializeJson(string json)
        {
            Scene scene = new Scene();

            JsonSerializer serializer = new JsonSerializer(CreateSettings(scene));
            using (System.Text.Json.JsonDocument reader = System.Text.Json.JsonDocument.Parse(json))
            {
                serializer.Deserialize(reader.RootElement, typeof(Scene));
                return scene;
            }
        }
    }
}
