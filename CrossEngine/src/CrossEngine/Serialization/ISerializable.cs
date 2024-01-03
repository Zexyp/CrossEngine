namespace CrossEngine.Serialization
{
    public interface ISerializable
    {
        internal protected void GetObjectData(SerializationInfo info);
        internal protected void SetObjectData(SerializationInfo info);
    }
}
