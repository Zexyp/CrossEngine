using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime;
using System.Threading.Tasks;
using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    public interface IAssetLoadContext
    {
        string GetFullPath(string realtivePath);
        Task<Stream> OpenStream(string path);
        
        Asset LoadChild(Type type, Guid id);
        void FreeChild(Asset asset);
        
        virtual T LoadChild<T>(Guid id) where T : Asset
        {
            return (T)LoadChild(typeof(T), id);
        }
    }

    public abstract class Asset : ISerializable
    {
        public Guid Id { get; internal set; }
        public abstract bool Loaded { get; }

        public abstract void Load(IAssetLoadContext context);
        public abstract void Unload(IAssetLoadContext context);

        public virtual void GetObjectData(SerializationInfo info) => info.AddValue(nameof(Id), Id);
        public virtual void SetObjectData(SerializationInfo info) => Id = info.GetValue(nameof(Id), Id);
    }
}
