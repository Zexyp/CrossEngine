using CrossEngine.Ecs;
using CrossEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossEngine.Assets
{
    interface IAssetCollection : ICollection<Asset>
    {
        void Add(Asset asset);
        void Remove(Asset asset);
        Asset Get(Guid id);
    }

    class AssetCollection<T> : IAssetCollection where T : Asset
    {
        Dictionary<Guid, T> _assets = new();
        bool _loaded = false;

        public int Count => _assets.Count;

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T asset)
        {
            if (asset.Id == Guid.Empty)
                asset.Id = Guid.NewGuid();

            _assets[asset.Id] = asset;
        }

        public bool Remove(T asset)
        {
            var result = _assets.Remove(asset.Id);

            return result;
        }

        void IAssetCollection.Add(Asset asset) => Add((T)asset);
        void IAssetCollection.Remove(Asset asset) => Remove((T)asset);

        public Asset Get(Guid id)
        {
            return _assets[id];
        }

        public void Add(Asset item)
        {
            Add((T)item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Asset item)
        {
            throw new NotImplementedException();
        }

        // 🤮
        public void CopyTo(Asset[] array, int arrayIndex) => _assets.Values.Select(a => (Asset)a).ToArray().CopyTo(array, arrayIndex);

        public bool Remove(Asset item)
        {
            return Remove((T)item);
        }

        public IEnumerator<Asset> GetEnumerator() => _assets.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _assets.Values.GetEnumerator();
    }
}
