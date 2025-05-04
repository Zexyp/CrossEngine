using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Ecs;
using CrossEngine.Logging;
using CrossEngine.Platform;
using CrossEngine.Serialization;
using CrossEngine.Core.Services;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using CrossEngine.Utils.Collections;

namespace CrossEngine.Assets
{
    public class AssetList : IAssetLoadContext, ISerializable
    {
        [EditorString]
        public string DirectoryOffset = "./";
        [EditorString]
        public string RuntimeFilepath;

        Dictionary<Type, Dictionary<Guid, Asset>> _collections = new();
        bool _loaded = false;
        
        public bool IsLoaded => _loaded;

        public void Add(Asset asset)
        {
            if (asset.Id == Guid.Empty)
                asset.Id = Guid.NewGuid();

            Type type = asset.GetType();
            if (!_collections.ContainsKey(type))
                _collections.Add(type, new Dictionary<Guid, Asset>());
            
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
            if (!type.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();

            if (_collections.ContainsKey(type))
                if (_collections[type].TryGetValue(id, out var asset))
                    return asset;

            foreach (var pair in _collections)
            {
                if (type.IsAssignableFrom(pair.Key))
                    if (pair.Value.TryGetValue(id, out var asset))
                        return asset;
            }
            
            throw new KeyNotFoundException();
        }
        
        public T GetNamed<T>(string name) where T : Asset => (T)GetNamed(typeof(T), name);

        public Asset GetNamed(Type type, string name)
        {
            if (!type.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();

            if (!_collections.ContainsKey(type))
                throw new KeyNotFoundException();

            foreach (Asset asset in _collections[type].Values)
            {
                if (asset.Name == name)
                    return asset;
            }
            throw new KeyNotFoundException();
        }

        public IEnumerable<T> GetCollection<T>() where T : Asset => new CastWrapCollection<T>(GetCollection(typeof(T)));
        public IEnumerable<Asset> GetCollection(Type ofType)
        {
            IEnumerable<Asset> coll = Enumerable.Empty<Asset>();
            foreach (var pair in _collections)
            {
                if (ofType.IsAssignableFrom(pair.Key))
                    coll = coll.Concat(pair.Value.Values);
            }

            return coll;
        }

        public bool TryGetCollection<T>(out IEnumerable<T> collection) where T : Asset
        {
            collection = default;
            var result = TryGetCollection(typeof(T), out var coll);
            if (result) collection = new CastWrapCollection<T>(coll);
            return result;
        }
        public bool TryGetCollection(Type ofType, out IEnumerable<Asset> collection)
        {
            collection = default;
            if (!HasCollection(ofType))
                return false;
            collection = GetCollection(ofType);
            return true;
        }

        public bool HasCollection<T>() where T : Asset => HasCollection(typeof(T));
        public bool HasCollection(Type ofType)
        {
            if (!ofType.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();

            if (_collections.ContainsKey(ofType))
                return true;
            
            foreach (var pair in _collections)
            {
                if (ofType.IsAssignableFrom(pair.Key))
                    return true;
            }
            
            return false;
        }

        // funny
        public IEnumerable<(Type, IEnumerable<Asset>)> Enumerate()
        {
            IEnumerable<Asset> InnerEnumerate(ICollection coll)
            {
                foreach (Asset item in coll)
                {
                    yield return item;
                }
            }

            foreach (var item in _collections)
            {
                yield return (item.Key, InnerEnumerate(item.Value.Values));
            }
        }

        public async Task<bool> LoadAll()
        {
            var result = true;
            foreach (var col in _collections.Values)
            {
                foreach (Asset asset in col.Values)
                {
                    if (!await LoadAsset(asset))
                        result = false;
                }
            }
            _loaded = true;

            return result;
        }

        public async Task<bool> UnloadAll()
        {
            var result = true;
            foreach (var col in _collections.Values)
            {
                foreach (Asset asset in col.Values)
                {
                    if (!await UnloadAsset(asset))
                        result = false;
                }
            }
            _loaded = false;

            return result;
        }

        public async Task<bool> LoadAsset(Asset asset)
        {
            try
            {
                if (!asset.Loaded)
                    await asset.Load(this);
                return true;
            }
            catch (Exception ex)
            {
                Log.Default.Error($"failed to load asset '{asset}': {ex.GetType().FullName}: {ex.Message}");
                Log.Default.Trace($"load asset '{asset}' failure details: {ex}");
                return false;
            }
        }

        public async Task<bool> UnloadAsset(Asset asset)
        {
            try
            {
                if (asset.Loaded)
                    await asset.Unload(this);
                return true;
            }
            catch (Exception ex)
            {
                Log.Default.Error($"failed to unload asset '{asset}': {ex.GetType().FullName}: {ex.Message}");
                Log.Default.Trace($"unload asset '{asset}' failure details: {ex}");
                return false;
            }
        }

        #region IAssetLoadContext
        Task<Stream> IAssetLoadContext.OpenStream(string path)
        {
            return PlatformHelper.FileRead(path);
        }

        string IAssetLoadContext.GetFullPath(string realtivePath)
        {
            return Path.Join(Path.GetDirectoryName(RuntimeFilepath), DirectoryOffset, realtivePath);
        }

        public Asset GetDependency(Type type, Guid id)
        {
            return Get(type, id);
        }
        #endregion

        #region ISerializable
        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue("DirectoryOffset", DirectoryOffset);
            List<Asset> imTooLazy = new();
            foreach (var col in _collections.Values)
                foreach (Asset asset in col.Values)
                    imTooLazy.Add(asset);
            info.AddValue("Assets", imTooLazy.ToArray());
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            DirectoryOffset = info.GetValue("DirectoryOffset", DirectoryOffset);

            var assets = info.GetValue<Asset[]>("Assets");
            for (int i = 0; i < assets.Length; i++)
            {
                Add(assets[i]);
            }
        }
        #endregion
    }
}
