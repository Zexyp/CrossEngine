using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    //enum AssetType
    //{
    //    Texture,
    //    Mesh
    //}

    public abstract class AssetInfo : ISerializable
    {
        //AssetType Type { get; }
        public string RelativePath { get; set; }
        //bool Active { get; set; }
        public bool Loaded { get; protected set; }

        public abstract void Load(IPathProvider pathProvider = null);
        public abstract void Unload();

        /*
        public uint Users { get; private set; }
        public void Lock()
        {
            if (Users == 0)
                OnUsed();
            Users++;
        }
        public void Unlock()
        {
            Users--;
            if (Users == 0)
                OnUnused();
        }
        protected virtual void OnUsed() { }
        protected virtual void OnUnused() { }
        */

        protected virtual void OnSerialize(SerializationInfo info) { }
        protected virtual void OnDeserialize(SerializationInfo info) { }

        void ISerializable.GetObjectData(SerializationInfo info)
        {
            info.AddValue(nameof(RelativePath), RelativePath);
            OnSerialize(info);
        }
        void ISerializable.SetObjectData(SerializationInfo info)
        {
            RelativePath = info.GetValue<string>(nameof(RelativePath));
            OnDeserialize(info);
        }
    }
}
