using CrossEngine.Assemblies;
using CrossEngine.Assets;
using CrossEngine.Scenes;
using CrossEngine.Serialization.Json;
using CrossEngine.Serialization;
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
        static readonly JsonConverter[] converters;
        static readonly TypeResolver resolver;

        private static readonly AssetGuidJsonConverter AssetConverter;

        static SceneSerializer()
        {
            resolver = new CrossAssemblyTypeResolver();
            converters = new JsonConverter[]
            {
                new EntityStructureJsonConverter(),

                AssetConverter = new AssetGuidJsonConverter(),

                new SceneJsonConverter(),
            };
        }

        public static void SerializeJson(Stream stream, Scene scene)
        {
            Serializer.SerializeJson(stream, scene, resolver: resolver, converters: converters);
        }

        public static Scene DeserializeJson(Stream stream, IAssetLoadContext assetContext)
        {
            AssetConverter.AssetContext = assetContext;

            return (Scene)Serializer.DeserializeJson(stream, typeof(Scene), resolver: resolver, converters: converters);
        }
    }
}
