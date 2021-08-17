using CrossEngine.Serialization.Json;

using CrossEngine.Utils.Editor;

namespace CrossEngine.Entities.Components
{
    public class TagComponent : Component, ISerializable
    {
        [EditorStringValue("Tag")]
        public string Tag = "";

        public TagComponent(string tag)
        {
            Tag = tag;
        }

        public TagComponent()
        {

        }

        #region ISerializable
        public void GetObjectData(SerializationInfo info)
        {
            // TODO: add resource serialization managment
            info.AddValue("Tag", Tag);
        }

        public TagComponent(DeserializationInfo info)
        {
            Tag = (string)info.GetValue("Tag", typeof(string));
        }
        #endregion
    }
}
