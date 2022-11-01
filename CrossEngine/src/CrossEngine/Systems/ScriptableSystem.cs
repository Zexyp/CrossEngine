using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;

namespace CrossEngine.Systems
{
    class ScriptableSystem : SimpleSystem<ScriptableComponent>
    {
        public override void Update()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Update();
            }
        }
    }
}
