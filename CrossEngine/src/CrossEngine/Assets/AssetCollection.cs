using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    public interface IAssetCollection : ICollection<AssetInfo>, ISerializable
    {
        void Load(IPathProvider pathProvider = null);
        void Unload();
        void Relativize(string toRoot);
    }

    public class AssetCollection<T> : IAssetCollection where T : AssetInfo
    {
        protected readonly Dictionary<string, AssetInfo> _assetDict = new Dictionary<string, AssetInfo>();
        bool Loaded = false;

        public void AddAsset(T asset)
        {
            _assetDict.Add(asset.RelativePath, asset);
            if (Loaded)
            {
                //asset.Active = true;
                asset.Load();
            }
        }

        public bool RemoveAsset(T asset)
        {
            if (_assetDict.ContainsKey(asset.RelativePath))
                return _assetDict.Remove(asset.RelativePath);
            return false;
        }

        public void Load(IPathProvider pathProvider = null)
        {
            foreach (var item in _assetDict)
            {
                item.Value.Load(pathProvider);
            }
            Loaded = true;
        }

        public void Unload()
        {
            foreach (var item in _assetDict)
            {
                //item.Value.Active = false;
                item.Value.Unload();
            }
            Loaded = false;
        }

        public void Relativize(string root)
        {
            foreach (var item in _assetDict.Values)
            {
                item.RelativePath = Path.GetRelativePath(root, item.RelativePath);
            }
        }

        #region ISerializeable
        public void GetObjectData(SerializationInfo info)
        {
            // also checks for users
            info.AddValue("Assets", _assetDict.Values.Where(a => a.Users > 0).ToArray());
        }

        public void SetObjectData(SerializationInfo info)
        {
            Debug.Assert(_assetDict.Count == 0);

            var asses = info.GetValue<AssetInfo[]>("Assets");
            for (int i = 0; i < asses.Length; i++)
            {
                _assetDict.Add(asses[i].RelativePath, (T)asses[i]);
            }
        }
        #endregion

        #region ICollection
        public int Count => _assetDict.Count;

        bool ICollection<AssetInfo>.IsReadOnly => ((ICollection<KeyValuePair<string, AssetInfo>>)_assetDict).IsReadOnly;

        void ICollection<AssetInfo>.Add(AssetInfo item) => AddAsset((T)item);

        public void Clear() => _assetDict.Clear();

        public bool Contains(AssetInfo item) => _assetDict.ContainsValue(item);

        public void CopyTo(AssetInfo[] array, int index) => _assetDict.Values.CopyTo(array, index);

        bool ICollection<AssetInfo>.Remove(AssetInfo item) => RemoveAsset((T)item);

        public IEnumerator<AssetInfo> GetEnumerator() => _assetDict.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _assetDict.Values.GetEnumerator();
        #endregion
    }
}
