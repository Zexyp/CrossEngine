using CrossEngine.Serialization;

using CrossEngine.Utils.Editor;

namespace CrossEngine.Entities.Components
{
    public class TagComponent : Component
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

        public override void OnSerialize(SerializationInfo info)
        {
            info.AddValue("Tag", Tag);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            Tag = (string)info.GetValue("Tag", typeof(string));
        }
    }
}
