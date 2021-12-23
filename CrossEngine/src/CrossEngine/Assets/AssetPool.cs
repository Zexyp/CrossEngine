using System;

using System.Collections.Generic;
using System.IO;

using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    public class AssetPool : ISerializable
    {
        public string Directory = "/.";

        //              Asset type
        //                  |        Collection
        //                  |            |
        private Dictionary<Type, IAssetCollection> _collections = new Dictionary<Type, IAssetCollection>();

        //string AssembliesListPath = "assemblies.list";

        //public string ResolveRelativePath(string fullpath) => PathUtils.GetRelativePath(Directory, fullpath);

        //public void AddAsset<T>(T asset) where T : Asset
        //{
        //    Type assetType = asset.GetType();
        //
        //    if (!_collections.ContainsKey(assetType)) AddCategory<T>();
        //
        //    _collections[assetType].Add(asset);
        //}
        //
        //public void RemoveAsset<T>(T asset) where T : Asset
        //{
        //    Type assetType = asset.GetType();
        //
        //    _collections[assetType].Remove(asset);
        //}

        private void AddCategory<T>() where T : Asset
        {
            Type type = typeof(T);
            if (!type.IsAssignableTo(typeof(Asset))) throw new InvalidOperationException($"Can't add type that doesn't implement inteface '{nameof(Asset)}'");
            if (_collections.ContainsKey(type)) throw new InvalidOperationException("There is already a collection of given type");
            _collections.Add(type, new AssetCollection<T>());
        }

        public AssetCollection<T> GetCollection<T>() where T : Asset
        {
            Type assetType = typeof(T);

            if (assetType == typeof(Asset)) throw new InvalidOperationException();

            if (!_collections.ContainsKey(assetType))
                AddCategory<T>();

            return (AssetCollection<T>)_collections[assetType];
        }

        public T? Get<T>(string name) where T : Asset
        {
            Type type = typeof(T);
            if (_collections.ContainsKey(type)) return null;

            return (T)_collections[type][name];
        }

        #region ISerializable
        public void OnSerialize(SerializationInfo info)
        {
            foreach (var col in _collections.Values)
            {
                col.Relativize(Directory);
            }
            info.AddValue("Data", _collections);
        }

        public void OnDeserialize(SerializationInfo info)
        {
            _collections = (Dictionary<Type, IAssetCollection>)info.GetValue("Data", typeof(Dictionary<Type, IAssetCollection>));
        }
        #endregion

        public void Load()
        {
            foreach (var col in _collections.Values)
            {
                col.Load();
            }
        }

        public void Unload()
        {
            foreach (var col in _collections.Values)
            {
                col.Unload();
            }
        }
    }
}
