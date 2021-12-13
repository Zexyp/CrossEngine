namespace CrossEngine.Serialization
{
    public interface ISerializable
    {
        public void OnSerialize(SerializationInfo info);
        public void OnDeserialize(SerializationInfo info);
    }
}