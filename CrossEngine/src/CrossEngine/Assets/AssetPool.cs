﻿using CrossEngine.Ecs;
using CrossEngine.Logging;
using CrossEngine.Platform;
using CrossEngine.Serialization;
using CrossEngine.Services;
using CrossEngine.Utils;
using CrossEngine.Utils.Editor;
using Silk.NET.Core.Native;
using System;
using System.Linq;
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
        [EditorString]
        public string Directory = "./";

        OrderedDictionary<Type, IDictionary> _collections = new();
        bool _loaded = false;
        Loader[] _loaders = null;

        public void Add(Asset asset)
        {
            if (asset.Id == Guid.Empty)
                asset.Id = Guid.NewGuid();

            Type type = asset.GetType();
            if (!_collections.ContainsKey(type))
            {
                var col = new Dictionary<Guid, Asset>();
                if (type.IsDefined(typeof(DependantAssetAttribute), false))
                    _collections.Add(type, col);
                else
                    _collections.Insert(0, type, col);
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
            if (!type.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();

            if (!_collections.ContainsKey(type))
                return null;

            return (Asset)_collections[type][id];
        }
        
        public T GetNamed<T>(string name) where T : Asset => (T)GetNamed(typeof(T), name);

        public Asset GetNamed(Type type, string name)
        {
            if (!type.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();

            if (!_collections.ContainsKey(type))
                return null;

            foreach (Asset asset in _collections[type].Values)
            {
                if (asset.Name == name)
                    return asset;
            }
            return null;
        }

        public ICollection<T>? GetCollection<T>() where T : Asset
        {
            var type = typeof(T);
            
            if (!_collections.ContainsKey(type))
                return null;

            return new CastWrapCollection<T>(_collections[type].Values);
        }

        public ICollection<Asset>? GetCollection(Type ofType)
        {
            if (!ofType.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();

            if (!_collections.ContainsKey(ofType))
                return null;

            return (ICollection<Asset>)_collections[ofType].Values;
        }

        public bool HasCollection<T>() where T : Asset
        {
            return _collections.ContainsKey(typeof(T));
        }
        public bool HasCollection(Type ofType)
        {
            if (!ofType.IsSubclassOf(typeof(Asset)))
                throw new InvalidOperationException();

            return _collections.ContainsKey(ofType);
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

            foreach (var item in (IDictionary<Type, IDictionary>)_collections)
            {
                yield return (item.Key, InnerEnumerate(item.Value.Values));
            }
        }

        public async Task<bool> LoadAll()
        {
            var result = true;
            foreach (IDictionary col in _collections.Values)
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
            foreach (IDictionary col in _collections.Values)
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

        public void BindLoaders(Loader[] loaders)
        {
            _loaders = loaders;
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
                AssetService.Log.Error($"failed to load asset '{asset}':\n{ex.GetType().FullName}: {ex.Message}");
                AssetService.Log.Trace($"load asset failiure details: {ex}");
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
                AssetService.Log.Error($"failed to unload asset '{asset}':\n{ex.GetType().FullName}: {ex.Message}");
                AssetService.Log.Trace($"unload asset failiure details: {ex}");
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
            return Path.Join(Directory, realtivePath);
        }

        async Task IAssetLoadContext.LoadChild(Type type, Guid id, Action<Asset> returnCallback)
        {
            var ch = (Asset)_collections[type][id];
            returnCallback.Invoke(ch);

            await LoadAsset(ch);
        }

        async Task IAssetLoadContext.FreeChild(Asset asset)
        {
            await UnloadAsset((Asset)_collections[asset.GetType()][asset.Id]);
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
            foreach (IDictionary col in _collections.Values)
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
