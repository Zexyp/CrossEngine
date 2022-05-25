using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using CrossEngine.Serialization;
using CrossEngine.Rendering.Textures;
using System.Collections;

namespace CrossEngine.Assets
{
    public class AssetRegistry : ISerializable, IPathProvider
    {
        private Dictionary<Type, IAssetCollection> _collections = new Dictionary<Type, IAssetCollection>();

        bool Loaded = false;

        string _homeDirectory;

        public AssetRegistry(string homeDirectory)
        {
            _homeDirectory = homeDirectory;
        }

        public IAssetCollection GetCollection(Type assetType)
        {
            if (_collections.TryGetValue(assetType, out var value))
                return value;
            var coll = (IAssetCollection)Activator.CreateInstance(typeof(AssetCollection<>).MakeGenericType(assetType));
            _collections.Add(assetType, coll);
            // TODO: add Add method
            if (Loaded)
                coll.Load();
            return coll;
        }

        public AssetCollection<T> GetCollection<T>() where T : AssetInfo
        {
            return (AssetCollection<T>)GetCollection(typeof(T));
        }

        public void GetObjectData(SerializationInfo info)
        {
            foreach (var item in _collections.Values)
            {
                item.Relativize(_homeDirectory);
            }
            info.AddValue(nameof(_collections), _collections);
        }

        public void SetObjectData(SerializationInfo info)
        {
            Debug.Assert(_collections.Count == 0);

            _collections = info.GetValue<Dictionary<Type, IAssetCollection>>(nameof(_collections));
        }

        public void Load()
        {
            foreach (var item in _collections)
            {
                item.Value.Load();
            }
            Loaded = true;
        }

        public void Unload()
        {
            foreach (var item in _collections)
            {
                item.Value.Unload();
            }
            Loaded = false;
        }

        string IPathProvider.GetActualPath(string relativePath) => $"{_homeDirectory}/{relativePath}";
    }

    class AssetManager
    {
        public static AssetRegistry _ctx;

        //public static TimeSpan CollectionTimeout = new TimeSpan(0, 0, 30);
        //static readonly Dictionary<IAssetInfo, DateTime> _collectedTextures = new Dictionary<IAssetInfo, DateTime>();

        public static AssetRegistry GetRegistry()
        {
            return _ctx;
        }

        //public static void CollectAsset(IAssetInfo texture)
        //{
        //    _collectedTextures.Add(texture, DateTime.Now);
        //}
        //
        //public static void CancelAssetCollect(IAssetInfo texture)
        //{
        //    _collectedTextures.Remove(texture);
        //}
        //
        //public static void Collect()
        //{
        //    var now = DateTime.Now;
        //
        //    List<IAssetInfo> bakys = new List<IAssetInfo>();
        //    foreach (var item in _collectedTextures)
        //    {
        //        if (now - item.Value >= CollectionTimeout) bakys.Add(item.Key);
        //    }
        //
        //    for (int i = 0; i < bakys.Count; i++)
        //    {
        //        bakys[i].Unload();
        //        _collectedTextures.Remove(bakys[i]);
        //    }
        //}
    }
}
