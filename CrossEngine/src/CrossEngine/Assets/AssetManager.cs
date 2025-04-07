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
        public static AssetList Current { get => _current; }
        
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
            using (Stream stream = await PlatformHelper.FileRead(filepath))
            {
                var pool = Read(stream);
                pool.Directory = Path.Join(Path.GetDirectoryName(filepath), pool.Directory);
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
                string prevdir = pool.Directory;
                pool.Directory = Path.GetRelativePath(Path.GetDirectoryName(filepath), pool.Directory);
                Write(pool, stream);
                pool.Directory = prevdir;
            }
        }

        public static void Write(AssetList pool, Stream stream)
        {
            JsonSerializer.Serialize(stream, pool, _jso);
        }
    }
}
