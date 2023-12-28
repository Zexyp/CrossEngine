using CrossEngine.Platform;
using CrossEngine.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class AssetPool : IAssetLoadContext, ISerializable
    {
        Dictionary<Type, IAssetCollection> _collections = new();
        string _directory = "./";
        bool _loaded = false;

        public string GetFullPath(string realtivePath)
        {
            return Path.Join(_directory, realtivePath);
        }

        Asset IAssetLoadContext.LoadChild(Type type, Guid id)
        {
            var ch = _collections[type].Get(id);
            if (!ch.Loaded)
                ch.Load(this);
            return ch;
        }

        void IAssetLoadContext.FreeChild(Asset asset)
        {
            if (asset.Loaded)
                _collections[asset.GetType()].Get(asset.Id).Unload(this);
        }

        Task<Stream> IAssetLoadContext.OpenStream(string path)
        {
            return PlatformHelper.FileOpen(path);
        }

        public void Add(Asset asset)
        {
            Type type = asset.GetType();
            if (!_collections.ContainsKey(type))
            {
                var col = (IAssetCollection)Activator.CreateInstance(typeof(AssetCollection<>).MakeGenericType(new[] { type }));
                _collections.Add(type, col);
            }
            _collections[type].Add(asset);
        }

        public void Remove(Asset asset)
        {
            Type type = asset.GetType();
            var col = _collections[type];
            col.Remove(asset);

            if (col.Count == 0)
                _collections.Remove(type);
        }

        public T Get<T>(Guid id) where T : Asset => (T)Get(typeof(T), id);

        public Asset Get(Type type, Guid id)
        {
            if (!_collections.ContainsKey(type))
                return null;
            return _collections[type].Get(id);
        }

        public void Load()
        {
            foreach (var item in _collections.Values)
            {
                foreach (var asset in item)
                {
                    if (!asset.Loaded)
                        asset.Load(this);
                }
            }
        }

        public void Unload()
        {
            foreach (var item in _collections.Values)
            {
                foreach (var asset in item)
                {
                    if (asset.Loaded)
                        asset.Unload(this);
                }
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue("Assets", _collections.Values.SelectMany(x => x).ToArray());
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            var assets = info.GetValue<Asset[]>("Assets");
            for (int i = 0; i < assets.Length; i++)
            {
                Add(assets[i]);
            }
        }
    }
}
