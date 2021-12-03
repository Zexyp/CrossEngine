using System;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using CrossEngine.Serialization;
using CrossEngine.Logging;

namespace CrossEngine.Assets
{
    public abstract class AssetCollection : ISerializable, IDisposable/*, ICollection<IAsset>*/
    {
        Dictionary<string, Asset> _assets = new Dictionary<string, Asset>();

        public event Action<AssetCollection, Asset> OnAssetAdded;
        public event Action<AssetCollection, Asset> OnAssetRemoved;

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
            
            OnAssetAdded?.Invoke(this, asset);
        }

        public void Remove(Asset asset)
        {
            asset.OnNameChanged -= OnAssetNameChanged;
            _assets.Remove(asset.Name);
            
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
            foreach (var ass in _assets.Values)
            {
                ass.Load();
            }
            Log.Core.Trace($"loaded asset collection of type '{this.GetType().GetGenericArguments()[0].Name}'");
        }

        public void Dispose()
        {
            foreach (var ass in _assets.Values)
            {
                ass.Dispose();
            }
            Log.Core.Trace($"unloaded asset collection of type '{this.GetType().GetGenericArguments()[0].Name}'");
        }
    }

    public class AssetCollection<T> : AssetCollection where T : Asset
    {

    }
}
