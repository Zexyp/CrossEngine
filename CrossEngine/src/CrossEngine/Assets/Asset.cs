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
        
        Asset LoadChild(Type type, Guid id);
        void FreeChild(Asset asset);

        Loader GetLoader(Type type);

        virtual Task<Stream> OpenRelativeStream(string realtivePath) => OpenStream(GetFullPath(realtivePath));

        virtual T GetLoader<T>() where T : Loader => (T)GetLoader(typeof(T));
        virtual T LoadChild<T>(Guid id) where T : Asset => (T)LoadChild(typeof(T), id);
    }

    public abstract class Asset : ISerializable
    {
        [EditorGuid]
        public Guid Id { get; internal set; }
        public abstract bool Loaded { get; }

        public abstract void Load(IAssetLoadContext context);
        public abstract void Unload(IAssetLoadContext context);

        public virtual void GetObjectData(SerializationInfo info) => info.AddValue(nameof(Id), Id);
        public virtual void SetObjectData(SerializationInfo info) => Id = info.GetValue(nameof(Id), Id);
    }
}
