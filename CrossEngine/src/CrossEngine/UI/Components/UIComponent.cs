using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrossEngine.ECS;
using CrossEngine.Events;

namespace CrossEngine.Components
{
    public abstract class UIComponent : Component
    {
        public virtual void OnEvent(Event e)
        {
            for (int i = 0; i < Entity.Children.Count; i++)
            {
                if (Entity.Children[i].TryGetComponent(out UIComponent uicomp))
                    uicomp.OnEvent(e);
            }
        }
    }
}
