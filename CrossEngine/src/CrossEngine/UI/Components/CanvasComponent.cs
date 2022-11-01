using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Events;
using CrossEngine.Systems;
using CrossEngine.Utils.Editor;

namespace CrossEngine.Components
{
    public class CanvasComponent : Component
    {
        [EditorValue]
        public bool DynamicSize;
        [EditorDrag]
        public Vector2 Size
        {
            get => _size;
            set
            {
                _size = value;
                Camera.Resize(_size.X, _size.Y);
            }
        }
        
        Vector2 _size = new Vector2(1, 1);
        internal Camera Camera = new Camera();

        protected internal override void Attach(World world)
        {
            world.GetSystem<UISystem>().RegisterCanvas(this);
        }

        protected internal override void Detach(World world)
        {
            world.GetSystem<UISystem>().UnregisterCanvas(this);
        }

        public void OnEvent(Event e)
        {
            if (e is WindowResizeEvent && DynamicSize)
            {
                var wre = (WindowResizeEvent)e;
                Size = new Vector2(wre.Width, wre.Height);
            }

            for (int i = 0; i < Entity.Children.Count; i++)
            {
                if (e.Handled) return;
                if (Entity.Children[i].TryGetComponent(out UIComponent uicomp))
                    uicomp.OnEvent(e);
            }
        }
    }
}
