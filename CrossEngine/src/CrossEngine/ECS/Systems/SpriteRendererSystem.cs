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
    class SpriteRendererSystem : System<SpriteRendererComponent>
    {
        public static SpriteRendererSystem Instance { get; private set; }
        public List<CrossEngine.Rendering.IObjectRenderData> Sus = new List<Rendering.IObjectRenderData>();

        public SpriteRendererSystem()
        {
            Debug.Assert(Instance == null);

            Instance = this;
        }

        public override void Update()
        {
            Sus.Clear();
            for (int i = 0; i < Components.Count; i++)
            {
                Sus.Add(Components[i]);
            }
        }
    }
}
