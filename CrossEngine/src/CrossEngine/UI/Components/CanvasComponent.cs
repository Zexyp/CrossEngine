using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Events;

namespace CrossEngine.Components
{
    public class CanvasComponent : Component
    {
        Camera _camera = new Camera();

        protected internal override void Attach(World world)
        {
            throw new NotImplementedException();
        }

        protected internal override void Detach(World world)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            _camera.ViewMatrix = Matrix4x4.CreateTranslation(-Entity.Transform?.WorldPosition ?? Vector3.Zero);
        }

        public void OnEvent(Event e)
        {
            for (int i = 0; i < Entity.Children.Count; i++)
            {
                if (e.Handled) return;
                if (Entity.Children[i].TryGetComponent(out UIComponent uicomp))
                    uicomp.OnEvent(e);
            }
        }
    }
}
