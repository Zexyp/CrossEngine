using CrossEngine.Assets;
using CrossEngine.Scenes;
using CrossEngine.Serialization.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace CrossEngine.Serialization
{
    public static class SceneSerializer
    {
        static SceneSerializer()
        {
            AssetJsonConverter assConv;
            options = new()
            {
#if DEBUG
                WriteIndented = true,
#endif
                Converters =
                {
                    new EntityStructureJsonConverter(),

                    (assConv = new AssetJsonConverter()),

                    new SceneJsonConverter(assConv),

                    new SerializableJsonConverter(),

                    new Vector2JsonConverter(),
                    new Vector3JsonConverter(),
                    new Vector4JsonConverter(),
                    new QuaternionJsonConverter(),
                    new Matrix4x4JsonConverter(),
                }
            };
        }

        static readonly JsonSerializerOptions options;

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
