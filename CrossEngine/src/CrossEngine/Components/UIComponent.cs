using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.Ecs;

namespace CrossEngine.Components
{
    public abstract class UIComponent : Component
    {
        internal protected abstract void Draw();
    }
}
