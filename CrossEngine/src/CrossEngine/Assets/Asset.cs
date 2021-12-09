using System;

using CrossEngine.Serialization;

namespace CrossEngine.Assets
{
    public abstract class Asset : ISerializable
    {
        public string Path { get; set; } = "";
        private string _name = "";
        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnNameChanged?.Invoke(this);
            }
        }

        public event Action<Asset> OnNameChanged;

        public abstract bool IsLoaded { get; protected set; }

        internal bool IsValid { get; set; }

        public virtual void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Path", Path);
            info.AddValue("Name", Name);
        }
        public virtual void OnDeserialize(SerializationInfo info)
        {
            Path = (string)info.GetValue("Path", typeof(string));
            Name = (string)info.GetValue("Name", typeof(string));
        }

        public virtual void Load()
        {
        }
        public virtual void Unload()
        {
        }
    }
}
