using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Assets
{
    public interface IAssetLoadContext
    {
        string GetFullPath(string realtivePath);
        Task<Stream> OpenStream(string path);

        Task LoadChild(Type type, Guid id, Action<Asset> returnCallback);
        Task FreeChild(Asset asset);

        Loader GetLoader(Type type);

        virtual Task<Stream> OpenRelativeStream(string realtivePath) => OpenStream(GetFullPath(realtivePath));

        virtual T GetLoader<T>() where T : Loader => (T)GetLoader(typeof(T));
        virtual async Task LoadChild<T>(Guid id, Action<T> returnCallback) where T : Asset
        {
            await LoadChild(typeof(T), id, a => returnCallback?.Invoke((T)a));
        }
    }

    public abstract class Asset : ISerializable
    {
        public Guid Id { get; internal set; }
        public abstract bool Loaded { get; }

        public abstract Task Load(IAssetLoadContext context);
        public abstract Task Unload(IAssetLoadContext context);

        public virtual void GetObjectData(SerializationInfo info) => info.AddValue(nameof(Id), Id);
        public virtual void SetObjectData(SerializationInfo info) => Id = info.GetValue(nameof(Id), Id);

        protected void SetChildId<T>(T value, ref T asset, ref Guid guid) where T : Asset
        {
            asset = value;
            if (asset != null) guid = asset.Id;
            else guid = Guid.Empty;
        }
    }

    public class DependantAssetAttribute : Attribute
    {

    }
}
