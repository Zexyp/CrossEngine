namespace CrossEngine.Serialization
{
    public interface ISerializable
    {
        public void GetObjectData(SerializationInfo info);
        public void SetObjectData(SerializationInfo info);
    }
}