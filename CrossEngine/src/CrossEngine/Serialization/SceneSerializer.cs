using CrossEngine.Scenes;
using CrossEngine.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace CrossEngine.Serialization
{
    public static class SceneSerializer
    {
        static readonly JsonSerializerOptions options = new JsonSerializerOptions()
        {
#if DEBUG
            WriteIndented = true,
#endif
            Converters =
                {
                    new Vector2JsonConverter(),
                    new Vector3JsonConverter(),
                    new Vector4JsonConverter(),
                    new QuaternionJsonConverter(),
                    new Matrix4x4JsonConverter(),

                    new EntityStructureJsonConverter(),
                    
                    new SerializableJsonConverter(),
                }
        };

        public static void Serialize(Stream stream, Scene scene)
        {
            lock (options)
            {
                for (int i = 0; i < options.Converters.Count; i++)
                {
                    if (options.Converters[i] is IInitializedConverter initMe)
                        initMe.Init();
                }

                JsonSerializer.Serialize(stream, scene, options);

                for (int i = 0; i < options.Converters.Count; i++)
                {
                    if (options.Converters[i] is IInitializedConverter finishMe)
                        finishMe.Finish();
                }
            }
        }

        public static Scene Deserialize(Stream stream)
        {
            Scene scene;
            
            lock (options)
            {
                for (int i = 0; i < options.Converters.Count; i++)
                {
                    if (options.Converters[i] is IInitializedConverter initMe)
                        initMe.Init();
                }

                scene = JsonSerializer.Deserialize<Scene>(stream, options);

                for (int i = 0; i < options.Converters.Count; i++)
                {
                    if (options.Converters[i] is IInitializedConverter finishMe)
                        finishMe.Finish();
                }
            }

            return scene;
        }
    }
}
