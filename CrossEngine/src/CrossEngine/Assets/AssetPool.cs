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
        public string Directory = "./";
        
        Dictionary<Type, IDictionary> _collections = new();
        bool _loaded = false;
        Loader[] _loaders = null;

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

        public void Load(Loader[] loaders)
        {
            _loaders = loaders;
            foreach (var col in _collections.Values)
            {
                foreach (Asset asset in col.Values)
                {
                    if (!asset.Loaded)
                        asset.Load(this);
                }
            }
            _loaded = true;
            _loaders = null;
        }

        public void Unload(Loader[] loaders)
        {
            _loaders = loaders;
            foreach (var col in _collections.Values)
            {
                foreach (Asset asset in col.Values)
                {
                    if (asset.Loaded)
                        asset.Unload(this);
                }
            }
            _loaded = false;
            _loaders = null;
        }

        #region IAssetLoadContext
        string IAssetLoadContext.GetFullPath(string realtivePath)
        {
            return Path.Join(Directory, realtivePath);
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

        public Loader GetLoader(Type type)
        {
            for (int i = 0; i < _loaders.Length; i++)
            {
                var l = _loaders[i];
                if (l.GetType() == type)
                    return l;
            }
            return null;
        }
        #endregion

        #region ISerializable
        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue("Directory", Directory);
            List<Asset> imTooLazy = new();
            foreach (var col in _collections.Values)
                foreach (Asset asset in col.Values)
                    imTooLazy.Add(asset);
            info.AddValue("Assets", imTooLazy.ToArray());
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            Directory = info.GetValue("Directory", Directory);

            var assets = info.GetValue<Asset[]>("Assets");
            for (int i = 0; i < assets.Length; i++)
            {
                Add(assets[i]);
            }
        }
        #endregion
    }
}
