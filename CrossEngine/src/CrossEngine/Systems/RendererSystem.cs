using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Numerics;

using CrossEngine.ECS;
using CrossEngine.Components;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Events;

namespace CrossEngine.Systems
{
    public class RendererSystem : ISystem
    {
        public SystemThreadMode ThreadMode => SystemThreadMode.Sync;

        List<CameraComponent> _cameras = new List<CameraComponent>();

        public CameraComponent PrimaryCamera;

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
                PrimaryCamera = component;
        }

        public void UnregisterCamera(CameraComponent component)
        {
            _cameras.Remove(component);
            if (PrimaryCamera == component)
                PrimaryCamera = null;
        }

        void ISystem.Event(object e)
        {
            if (e is WindowResizeEvent)
            {
                Application.CoreLog.Debug("TODO: camera window resize event");
            }
        }
    }
}
