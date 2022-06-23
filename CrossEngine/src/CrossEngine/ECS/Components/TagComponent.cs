using CrossEngine.ECS;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;
using CrossEngine.ComponentSystems;

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

        protected internal override void Attach(World world)
        {
            world.GetSystem<TagSystem>().Register(this);
        }

        protected internal override void Detach(World world)
        {
            world.GetSystem<TagSystem>().Unregister(this);
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
