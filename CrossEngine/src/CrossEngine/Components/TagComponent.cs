using System;

using CrossEngine.ECS;
using CrossEngine.Utils.Editor;
using CrossEngine.Serialization;
using CrossEngine.Systems;

namespace CrossEngine.Components
{
    public class TagComponent : Component
    {
        [EditorString(256)]
        public string Tag
        {
            get => _tag;
            set
            {
                if (value == _tag) return;

                var old = _tag;
                _tag = value;
                OnTagChanged?.Invoke(this, old, value);
            }
        }

        public event Action<TagComponent, string, string> OnTagChanged;
        private string _tag = "";

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
            Tag = info.GetValue(nameof(Tag), Tag);
        }
    }
}
