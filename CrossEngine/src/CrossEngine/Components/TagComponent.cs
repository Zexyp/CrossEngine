using CrossEngine.Ecs;
using CrossEngine.Serialization;
using CrossEngine.Utils.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Components
{
    public class TagComponent : Component
    {
        private string tag = "";
        [EditorString]
        public string Tag
        {
            get => tag;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                tag = value;
            }
        }

        protected internal override void OnSerialize(SerializationInfo info)
        {
            base.OnSerialize(info);

            info.AddValue(nameof(Tag), Tag);
        }

        protected internal override void OnDeserialize(SerializationInfo info)
        {
            base.OnDeserialize(info);

            Tag = info.GetValue(nameof(Tag), Tag);
        }
    }
}
