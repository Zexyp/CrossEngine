using CrossEngine.Assemblies;
using CrossEngine.Assets;
using CrossEngine.Scenes;
using CrossEngine.Serialization.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossEngine.Serialization
{
    public static class SceneSerializer
    {
        static readonly JsonSerializerOptions options;
        static readonly TypeResolver resolver;

        static SceneSerializer()
        {
            AssetGuidJsonConverter assConv;
            
            options = new()
            {
#if DEBUG
                WriteIndented = true,
#endif
            };

            foreach (var item in new JsonConverter[]
                {
                    new EntityStructureJsonConverter(),

                    new AssetGuidJsonConverter(),

                    new SceneJsonConverter(),
                }.Concat(Serialization.Serializer.BaseJsonConverters))
            {
                options.Converters.Add(item);
            }

            resolver = new CrossAssemblyTypeResolver();
            for (int i = 0; i < options.Converters.Count; i++)
            {
                if (options.Converters[i] is ITypeResolveConverter resolveMe)
                    resolveMe.Resolver = resolver;
            }
        }

        public static void SerializeJson(Stream stream, Scene scene)
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

        public static Scene DeserializeJson(Stream stream)
        {
            Scene scene;
            
            Debug.Assert(AssetManager.Current != null, "no asset lookup");
            
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
