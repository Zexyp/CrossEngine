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

namespace CrossEngine.Assets
{
    public static class AssetManager
    {
        public static AssetList Current { get => _current; }

        internal static Func<Action, Task> ServiceRequest;

        private static AssetList _current;
        private static JsonSerializerOptions _jso;

        static AssetManager()
        {
            _jso = new JsonSerializerOptions();
            foreach (var item in Serialization.Serializer.BaseJsonConverters)
            {
                _jso.Converters.Add(item);
            }

#if DEBUG
            _jso.WriteIndented = true;
#endif
        }

        public static T Get<T>(Guid id) where T : Asset => _current.Get<T>(id);
        public static Asset Get(Type typeOfAsset, Guid id) => _current.Get(typeOfAsset, id);

        public static T GetNamed<T>(string name) where T : Asset => _current.GetNamed<T>(name);
        public static Asset GetNamed(Type typeOfAsset, string name) => _current.GetNamed(typeOfAsset, name);

        public static void Bind(AssetList pool)
        {
            _current = pool;
        }

        public static async Task<AssetList> ReadFile(string filepath)
        {
            using (Stream stream = await PlatformHelper.FileReadAsync(filepath))
            {
                var pool = Read(stream);
                pool.RuntimeFilepath = filepath;
                return pool;
            }
        }

        public static AssetList Read(Stream stream)
        {
            return JsonSerializer.Deserialize<AssetList>(stream, _jso);
        }

        public static void WriteFile(AssetList pool, string filepath)
        {
            using (Stream stream = PlatformHelper.FileCreate(filepath))
            {
                pool.RuntimeFilepath = filepath;
                Write(pool, stream);
            }
        }

        public static void Write(AssetList pool, Stream stream)
        {
            JsonSerializer.Serialize(stream, pool, _jso);
        }

        public static Task Load(AssetList list)
        {
            Debug.Assert(list != null);
            return ServiceRequest.Invoke(async () => await list.LoadAll());
        }
        public static Task Unload(AssetList list)
        {
            Debug.Assert(list != null);
            return ServiceRequest.Invoke(async () => await list.UnloadAll());
        }
    }
}
