using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using CrossEngine.Serialization;
using CrossEngine.Logging;
using System.Collections;

namespace CrossEngine.Assets
{
    public abstract class AssetCollection : ISerializable/*, ICollection<Asset>*/
    {
        Dictionary<string, Asset> _assets = new Dictionary<string, Asset>();

        public event Action<AssetCollection, Asset> OnAssetAdded;
        public event Action<AssetCollection, Asset> OnAssetRemoved;

        // TODO: check if this makes sense
        private bool loaded = false;

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public Asset this[string name]
        {
            get
            {
                if (_assets.TryGetValue(name, out Asset asset)) return asset;
                return _assets[name];
            }
        }

        public IReadOnlyCollection<Asset> GetAll()
        {
            return _assets.Values;
        }

        public void Add(Asset asset)
        {
            if (!CheckNameAvailability(asset.Name, out string newName)) asset.Name = newName;

            asset.OnNameChanged += OnAssetNameChanged;
            _assets.Add(asset.Name, asset);
            
            asset.IsValid = true;

            if (loaded && !asset.IsLoaded)
                asset.Load();

            OnAssetAdded?.Invoke(this, asset);
        }

        public void Remove(Asset asset)
        {
            asset.OnNameChanged -= OnAssetNameChanged;
            _assets.Remove(asset.Name);

            if (loaded && asset.IsLoaded)
                asset.Unload();

            asset.IsValid = false;

            OnAssetRemoved?.Invoke(this, asset);
        }

        public void Clear()
        {
            throw new NotImplementedException();

            while (_assets.Count > 0)
            {
                var asset = System.Linq.Enumerable.ElementAt(_assets, 0).Value;
                Remove(asset);
            }
        }

        public void Relativize(string directory)
        {
            foreach (var item in _assets.Values)
            {
                item.Path = Path.GetRelativePath(directory, item.Path);
            }
        }

        // true if available
        private bool CheckNameAvailability(string name, out string nextAvailableName)
        {
            if (!_assets.ContainsKey(name))
            {
                nextAvailableName = null;
                return true;
            }

            int i = 1;
            string newName;
            do
            {
                newName = name + "." + i.ToString("D3");
                i++;
            }
            while (_assets.ContainsKey(newName));

            nextAvailableName = newName;
            return false;
        }

        private void OnAssetNameChanged(Asset sender)
        {
            string oldKey = null;
            foreach (var pair in _assets)
            {
                if (pair.Value == sender)
                {
                    oldKey = pair.Key;
                    break;
                }
            }
            _assets.Remove(oldKey);

            if(!CheckNameAvailability(sender.Name, out string nextName))
            {
                var existing = _assets[sender.Name];
                _assets.Remove(sender.Name);

                existing.OnNameChanged -= OnAssetNameChanged;
                existing.Name = nextName;
                existing.OnNameChanged += OnAssetNameChanged;

                _assets.Add(existing.Name, existing);
            }

            _assets.Add(sender.Name, sender);
        }

        #region ISerializable
        public virtual void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Collection", _assets);
        }

        public virtual void OnDeserialize(SerializationInfo info)
        {
            _assets = (Dictionary<string, Asset>)info.GetValue("Collection", typeof(Dictionary<string, Asset>));
        }
        #endregion

        public void Load()
        {
            loaded = true;

            foreach (var ass in _assets.Values)
            {
                ass.Load();
            }
            Log.Core.Trace($"loaded asset collection of type '{this.GetType().GetGenericArguments()[0].Name}'");
        }

        public void Unload()
        {
            loaded = false;

            foreach (var ass in _assets.Values)
            {
                ass.Unload();
            }
            Log.Core.Trace($"unloaded asset collection of type '{this.GetType().GetGenericArguments()[0].Name}'");
        }

        /*
        public bool Contains(Asset item) => ((ICollection<Asset>)_assets.Values).Contains(item);

        public void CopyTo(Asset[] array, int index) => ((ICollection<Asset>)_assets.Values).CopyTo(array, index);

        public IEnumerator<Asset> GetEnumerator() => ((ICollection<Asset>)_assets.Values).GetEnumerator();

        bool ICollection<Asset>.Remove(Asset item) => ((ICollection<Asset>) _assets.Values).Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => ((ICollection<Asset>)_assets.Values).GetEnumerator();
        */
    }

    public class AssetCollection<T> : AssetCollection where T : Asset
    {

    }
}
