using CrossEngine.Logging;
using CrossEngine.Platform;
using CrossEngine.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public static class AssetManager
    {
        public static AssetPool Current { get => _current; }
        
        internal static AssetService service;
        
        private static AssetPool _current;
        private static JsonSerializerOptions _jso;

        static AssetManager()
        {
            _jso = new JsonSerializerOptions();
            foreach (var item in CrossEngine.Serialization.SceneSerializer.BaseConverters)
            {
                _jso.Converters.Add(item);
            }

#if DEBUG
            _jso.WriteIndented = true;
#endif
        }

        public static T Get<T>(Guid id) where T : Asset
        {
            return (T)Get(typeof(T), id);
        }

        public static Asset Get(Type typeOfAsset, Guid id)
        {
            return _current.Get(typeOfAsset, id);
        }

        public static void Bind(AssetPool pool)
        {
            _current?.BindLoaders(null);
            _current = pool;
            _current?.BindLoaders(service.Loaders.ToArray());
        }

        public static async Task<AssetPool> ReadFile(string filepath)
        {
            AssetService.Log.Trace($"read file '{filepath}'");
            using (Stream stream = await PlatformHelper.FileRead(filepath))
            {
                var pool = JsonSerializer.Deserialize<AssetPool>(stream, _jso);
                pool.Directory = Path.Join(Path.GetDirectoryName(filepath), pool.Directory);
                return pool;
            }
        }

        public static void WriteFile(AssetPool pool, string filepath)
        {
            AssetService.Log.Trace($"write file '{filepath}'");
            using (Stream stream = PlatformHelper.FileWrite(filepath))
            {
                string prevdir = pool.Directory;
                pool.Directory = Path.GetRelativePath(Path.GetDirectoryName(filepath), pool.Directory);
                JsonSerializer.Serialize(stream, pool, _jso);
                pool.Directory = prevdir;
            }
        }

        public static void Load()
        {
            service.LoadAsync(_current).Wait();
        }

        public static void Unload()
        {
            service.UnloadAsync(_current).Wait();
        }

        public static Task LoadAsync()
        {
            return service.LoadAsync(_current);
        }

        public static Task UnloadAsync()
        {
            return service.UnloadAsync(_current);
        }
    }
}
