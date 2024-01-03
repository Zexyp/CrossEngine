using CrossEngine.Ecs;
using CrossEngine.Platform;
using CrossEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    public class AssetPool : IAssetLoadContext, ISerializable
    {
        Dictionary<Type, IDictionary> _collections = new();
        string _directory = "./";
        bool _loaded = false;

        #region IAssetLoadContext
        string IAssetLoadContext.GetFullPath(string realtivePath)
        {
            return Path.Join(_directory, realtivePath);
        }

        Asset IAssetLoadContext.LoadChild(Type type, Guid id)
        {
            var ch = (Asset)_collections[type][id];
            if (!ch.Loaded)
                ch.Load(this);
            return ch;
        }

        void IAssetLoadContext.FreeChild(Asset asset)
        {
            if (asset.Loaded)
                ((Asset)_collections[asset.GetType()][asset.Id]).Unload(this);
        }
        #endregion

        Task<Stream> IAssetLoadContext.OpenStream(string path)
        {
            return PlatformHelper.FileOpen(path);
        }

        public void Add(Asset asset)
        {
            if (asset.Id == Guid.Empty)
                asset.Id = Guid.NewGuid();

            Type type = asset.GetType();
            if (!_collections.ContainsKey(type))
            {
                var col = new Dictionary<Guid, Asset>();
                _collections.Add(type, col);
            }
            _collections[type].Add(asset.Id, asset);
        }

        public void Remove(Asset asset)
        {
            Type type = asset.GetType();
            var col = _collections[type];
            col.Remove(asset.Id);

            if (col.Count == 0)
                _collections.Remove(type);
        }

        public T Get<T>(Guid id) where T : Asset => (T)Get(typeof(T), id);

        public Asset Get(Type type, Guid id)
        {
            if (!_collections.ContainsKey(type))
                return null;
            return (Asset)_collections[type][id];
        }

        public ICollection<T>? GetCollection<T>() where T : Asset
        {
            return (ICollection<T>)GetCollection(typeof(T));
        }

        public ICollection? GetCollection(Type ofType)
        {
            if (!ofType.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();
            if (!_collections.ContainsKey(ofType))
                return null;
            return _collections[ofType].Values;
        }

        public void Load()
        {
            foreach (var col in _collections.Values)
            {
                foreach (Asset asset in col.Values)
                {
                    if (!asset.Loaded)
                        asset.Load(this);
                }
            }
        }

        public void Unload()
        {
            foreach (var col in _collections.Values)
            {
                foreach (Asset asset in col.Values)
                {
                    if (asset.Loaded)
                        asset.Unload(this);
                }
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info)
        {
            List<Asset> imTooLazy = new();
            foreach (var col in _collections.Values)
                foreach (Asset asset in col.Values)
                    imTooLazy.Add(asset);
            info.AddValue("Assets", imTooLazy.ToArray());
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
