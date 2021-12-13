using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Newtonsoft.Json;

using CrossEngine.Scenes;
using CrossEngine.Serialization.Json;
using CrossEngine.Serialization.Json.Converters;

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

        private static JsonSerializerSettings CreateSettings(Scene scene)
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented,
                Converters = {
                    //new ArrayReferencePreservngConverter(),
                    new SerializableInterfaceJsonConverter(),
                    
                    new Vector2JsonConverter(),
                    new Vector3JsonConverter(),
                    new Vector4JsonConverter(),
                    new QuaternionJsonConverter(),
                    new Matrix4x4JsonConverter(),
                    
                    new SceneJsonConverter(scene),
                    new EntityJsonConverter(scene),
                    new ComponentJsonConverter(),
                }
            };
        }

        public static string SertializeJson(Scene scene)
        {
            JsonSerializer serializer = JsonSerializer.Create(CreateSettings(scene));
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(stream))
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

            JsonSerializer serializer = JsonSerializer.Create(CreateSettings(scene));
            using (MemoryStream stream = new MemoryStream(Encoding.Default.GetBytes(json)))
            using (StreamReader reader = new StreamReader(stream))
            {
                serializer.Deserialize(reader, typeof(Scene));
                return scene;
            }
        }
    }
}
