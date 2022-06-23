using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;

namespace CrossEngine.ComponentSystems
{
    public class TagSystem : System<TagComponent>
    {
        // TODO: better structure (Dict<string, List<Comp>>)
        public TagComponent[] GetByTag(string tag)
        {
            List<TagComponent> list = new List<TagComponent>();
            for (int i = 0; i < Components.Count; i++)
            {
                if (tag == Components[i].Tag)
                    list.Add(Components[i]);
            }
            return list.ToArray();
        }
    }
}
