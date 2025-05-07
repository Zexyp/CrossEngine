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

        Asset GetDependency(Type type, Guid id);
        virtual T GetDependency<T>(Guid id) where T : Asset => (T)GetDependency(typeof(T), id);

        virtual Task<Stream> OpenRelativeStream(string realtivePath) => OpenStream(GetFullPath(realtivePath));
    }

    public abstract class Asset : ISerializable
    {
        public Guid Id { get; internal set; }
        public abstract bool Loaded { get; }
        [EditorString]
        public string Name;

        public abstract Task Load(IAssetLoadContext context);
        public abstract Task Unload(IAssetLoadContext context);

        public virtual void GetObjectData(SerializationInfo info)
        {
            info.AddValue(nameof(Id), Id);
            info.AddValue(nameof(Name), Name);
        }
        public virtual void SetObjectData(SerializationInfo info)
        {
            Id = info.GetValue(nameof(Id), Id);
            Name = info.GetValue(nameof(Name), Name);
        }

        protected void SetChildId<T>(T value, ref T asset, ref Guid guid) where T : Asset
        {
            asset = value;
            if (asset != null) guid = asset.Id;
            else guid = Guid.Empty;
        }

        public virtual string GetName() => Name ?? Id.ToString();
    }

    public abstract class FileAsset : Asset
    {
        [EditorString]
        public string RelativePath;

        public override void GetObjectData(SerializationInfo info)
        {
            base.GetObjectData(info);

            info.AddValue(nameof(RelativePath), RelativePath);
        }

        public override void SetObjectData(SerializationInfo info)
        {
            base.SetObjectData(info);

            RelativePath = info.GetValue(nameof(RelativePath), RelativePath);
        }

        public override string GetName() => Name ?? RelativePath ?? Id.ToString();
    }
}
