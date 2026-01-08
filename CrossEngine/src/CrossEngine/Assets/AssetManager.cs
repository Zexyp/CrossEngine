using CrossEngine.Logging;
using CrossEngine.Platform;
using CrossEngine.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CrossEngine.Rendering;
using CrossEngine.Serialization;
using CrossEngine.Serialization.Json;

namespace CrossEngine.Assets
{
    public static class AssetManager
    {
        public static AssetList Current { get => _current; }
        
        internal static Func<AssetList, Task> LoadRequest;
        internal static Func<AssetList, Task> UnloadRequest;

        private static AssetList _current;

        public static async Task<AssetList> ReadFile(string filepath)
        {
            using (Stream stream = await PlatformHelper.FileReadAsync(filepath))
            {
                var pool = Read(stream);
                pool.Context.Path = Path.GetDirectoryName(filepath);
                return pool;
            }
        }

        public static AssetList Read(Stream stream)
        {
            return (AssetList)Serializer.DeserializeJson(stream, typeof(AssetList));
        }

        public static void WriteFile(AssetList pool, string filepath)
        {
            using (Stream stream = PlatformHelper.FileCreate(filepath))
            {
                pool.Context.Path = Path.GetDirectoryName(filepath);
                Write(pool, stream);
            }
        }

        public static void Write(AssetList pool, Stream stream)
        {
            Serializer.SerializeJson(stream, pool);
        }

        public static Task Load(AssetList list) => LoadRequest(list);
        public static Task Unload(AssetList list) => UnloadRequest(list);
        
        public static T Get<T>(Guid id) where T : Asset => _current.Get<T>(id);
        public static Asset Get(Type typeOfAsset, Guid id) => _current.Get(typeOfAsset, id);

        public static T GetNamed<T>(string name) where T : Asset => _current.GetNamed<T>(name);
        public static Asset GetNamed(Type typeOfAsset, string name) => _current.GetNamed(typeOfAsset, name);
    }
}
