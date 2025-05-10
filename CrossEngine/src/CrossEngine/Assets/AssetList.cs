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
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Security.Cryptography;

namespace CrossEngine.Assets
{
    public class AssetList : IAssetLoadContext, ISerializable
    {
        [EditorString]
        public string DirectoryOffset = "./";
        [EditorString]
        public string RuntimeFilepath;

        public bool IsLoaded => _loaded;

        Dictionary<Type, Dictionary<Guid, Asset>> _collections = new();
        Dictionary<string, List<FileAsset>> _fileAssets = new(new PathEqualityComparer());
        
        bool _loaded = false;

        public void Add(Asset asset)
        {
            if (asset.Id == Guid.Empty)
                asset.Id = Guid.NewGuid();

            Type type = asset.GetType();
            if (!_collections.ContainsKey(type))
                _collections.Add(type, new Dictionary<Guid, Asset>());
            
            _collections[type].Add(asset.Id, asset);

            if (asset is FileAsset fasset)
            {
                if (fasset.RelativePath != null)
                {
                    // add
                    if (!_fileAssets.TryGetValue(fasset.RelativePath, out var list))
                        _fileAssets.Add(fasset.RelativePath, list = new());

                    list.Add(fasset);
                }

                fasset.RelativePathChanged += OnFileAssetPathChanged;
            }
        }

        public void Remove(Asset asset)
        {
            if (asset is FileAsset fasset)
            {
                fasset.RelativePathChanged -= OnFileAssetPathChanged;

                if (fasset.RelativePath != null)
                {
                    // remove
                    if (!_fileAssets.TryGetValue(fasset.RelativePath, out var list))
                    {
                        list.Remove(fasset);
                        if (list.Count == 0)
                            _fileAssets.Remove(fasset.RelativePath);
                    }
                }
            }

            Type type = asset.GetType();
            
            var col = _collections[type];
            var result = col.Remove(asset.Id);
            Debug.Assert(result);

            if (col.Count == 0)
                _collections.Remove(type);
        }

        public T Get<T>(Guid id) where T : Asset => (T)Get(typeof(T), id);

        public Asset Get(Type type, Guid id)
        {
            if (!type.IsSubclassOf(typeof(Asset)))
                throw new ArgumentException();

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
                throw new ArgumentException();

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
            foreach (KeyValuePair<Type, Dictionary<Guid, Asset>> pair in _collections)
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
            
            foreach (KeyValuePair<Type, Dictionary<Guid, Asset>> pair in _collections)
            {
                if (ofType.IsAssignableFrom(pair.Key))
                    return true;
            }
            
            return false;
        }

        //public void MoveCollection(Type typeOfCollection, int index)
        //{
        //    Debug.Assert(_collections.Contains(typeOfCollection));
        //    _collections.Remove(typeOfCollection, out var value);
        //    _collections.Insert(index, typeOfCollection, value);
        //}

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
            foreach (var col in _collections.Reverse().Select(p => p.Value))
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
            }
            catch (Exception ex)
            {
                Log.Default.Error($"failed to load asset '{asset}': {ex.GetType().FullName}:\n{ex.Message}");
                Log.Default.Trace($"load asset '{asset}' failure details: {ex}");
                return false;
            }

            return true;
        }

        public async Task<bool> UnloadAsset(Asset asset)
        {
            try
            {
                if (asset.Loaded)
                    await asset.Unload(this);
            }
            catch (Exception ex)
            {
                Log.Default.Error($"failed to unload asset '{asset}': {ex.GetType().FullName}:\n{ex.Message}");
                Log.Default.Trace($"unload asset '{asset}' failure details: {ex}");
                return false;
            }

            return true;
        }

        private void OnFileAssetPathChanged(FileAsset fasset, string old)
        {
            if (old != null)
            {
                // remove
                if (!_fileAssets.TryGetValue(old, out var list))
                {
                    list.Remove(fasset);
                    if (list.Count == 0)
                        _fileAssets.Remove(old);
                }
            }
            
            if (fasset.RelativePath != null)
            {
                // add
                if (!_fileAssets.TryGetValue(fasset.RelativePath, out var list))
                    _fileAssets.Add(fasset.RelativePath, list = new());

                list.Add(fasset);
            }
        }

        #region IAssetLoadContext
        Task<Stream> IAssetLoadContext.OpenStream(string path)
        {
            return PlatformHelper.FileReadAsync(path);
        }

        string IAssetLoadContext.GetFullPath(string realtivePath)
        {
            return Path.Join(Path.GetDirectoryName(RuntimeFilepath), DirectoryOffset, realtivePath);
        }

        public Asset GetDependency(Type type, Guid id)
        {
            var asset = Get(type, id);
            if (asset?.Loaded == false) LoadAsset(asset);
            return asset;
        }

        public Asset GetFileAsset(Type type, string file)
        {
            Asset result = null;
            if (_fileAssets.TryGetValue(file, out var ls))
                result = ls.FirstOrDefault();
            LoadAsset(result);
            return result;
        }
        #endregion

        #region ISerializable
        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue(nameof(DirectoryOffset), DirectoryOffset);
            List<Asset> imTooLazy = new();
            foreach (var col in _collections.Values)
                foreach (Asset asset in col.Values)
                    imTooLazy.Add(asset);
            info.AddValue("Assets", imTooLazy.ToArray());
        }

        void ISerializable.SetObjectData(SerializationInfo info)
        {
            DirectoryOffset = info.GetValue(nameof(DirectoryOffset), DirectoryOffset);

            var assets = info.GetValue<Asset[]>("Assets");
            for (int i = 0; i < assets.Length; i++)
            {
                Add(assets[i]);
            }
        }
        #endregion

        private class PathEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string path1, string path2)
            {
                string norm1 = NormalizePath(path1);
                string norm2 = NormalizePath(path2);

                return string.Equals(norm1, norm2);
            }

            public int GetHashCode(string path)
            {
                string norm = NormalizePath(path);
                return norm.GetHashCode();
            }

            private string NormalizePath(string path)
            {
                return path.Replace("\\", "/");
            }
        }
    }
}
