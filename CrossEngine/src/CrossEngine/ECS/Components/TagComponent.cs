using CrossEngine.ECS;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;

namespace CrossEngine.Components
{
    public class TagComponent : Component
    {
        [EditorString(256)]
        public string Tag = "";

        public TagComponent()
        {

        }

        public TagComponent(string tag)
        {
            Tag = tag;
        }

        protected override Component CreateClone()
        {
            return new TagComponent(Tag);
        }

        protected internal override void Serialize(SerializationInfo info)
        {
            info.AddValue(nameof(Tag), Tag);
        }

        protected internal override void Deserialize(SerializationInfo info)
        {
            Tag = info.GetValue<string>(nameof(Tag));
        }
    }
}
