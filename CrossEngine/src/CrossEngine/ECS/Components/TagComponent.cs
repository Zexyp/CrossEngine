using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.ECS;

namespace CrossEngine.Components
{
    public class TagComponent : Component
    {
        public string Tag = "";

        public TagComponent()
        {

        }

        public TagComponent(string tag)
        {
            Tag = tag;
        }

        public override object Clone()
        {
            return new TagComponent(Tag);
        }
    }
}
