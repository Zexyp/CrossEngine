using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Events;

namespace CrossEngine.ComponentSystems
{
    class RendererSystem : ISystem
    {
        public SystemThreadMode ThreadMode => SystemThreadMode.Sync;

        List<CameraComponent> _cameras = new List<CameraComponent>();

        public CameraComponent Primary;

        public void Init()
        {

        }

        public void Shutdown()
        {

        }

        public void Update()
        {
            
        }

        public void RegisterCamera(CameraComponent component)
        {
            _cameras.Add(component);
            if (component.Primary)
                Primary = component;
        }

        public void UnregisterCamera(CameraComponent component)
        {
            _cameras.Remove(component);
            if (Primary == component)
                Primary = null;
        }

        void ISystem.Event(object e)
        {
            if (e is WindowResizeEvent)
            {
                Logging.Log.Core.Debug("TODO: camera window resize event");
            }
        }
    }
}
