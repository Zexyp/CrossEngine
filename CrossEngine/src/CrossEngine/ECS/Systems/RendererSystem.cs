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

        public static RendererSystem Instance;

        List<CameraComponent> _cameras = new List<CameraComponent>();

        public CameraComponent Primary;

        public RendererSystem()
        {
            Debug.Assert(Instance == null);

            Instance = this;
        }

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
        }

        public void UnregisterCamera(CameraComponent component)
        {
            _cameras.Remove(component);
        }
    }
}
