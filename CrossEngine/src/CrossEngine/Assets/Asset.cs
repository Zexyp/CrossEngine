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
        Stream OpenStream(string path);
        
        void LoadChild(Type type, in Guid id, out Asset to);
        void FreeChild(Asset asset);

        Loader GetLoader(Type type);

        virtual Stream OpenRelativeStream(string realtivePath) => OpenStream(GetFullPath(realtivePath));

        virtual T GetLoader<T>() where T : Loader => (T)GetLoader(typeof(T));
        virtual void LoadChild<T>(in Guid id, out T to) where T : Asset
        {
            LoadChild(typeof(T), id, out var o);
            to = (T)o;
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
