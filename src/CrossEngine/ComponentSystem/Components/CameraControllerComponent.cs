using System;

using System.Numerics;

using CrossEngine.Rendering.Cameras;
using CrossEngine.Audio;

namespace CrossEngine.ComponentSystem.Components
{
    // also controls the listener
    public class CameraControllerComponent : Component
    {
        public Camera camera;

        public CameraControllerComponent(Camera camera)
        {
            this.camera = camera;
            Events.OnWindowResized += OnWindowResized;
        }

        Vector3 lastPosition;
        public override void OnUpdate(float timestep)
        {
            AudioListener.Position = camera.Transform.Position;
            AudioListener.SetOrientation(camera.Front, camera.Up);
            AudioListener.Velocity = (camera.Transform.Position - lastPosition) / timestep;
            lastPosition = camera.Transform.Position;
        }

        private void OnWindowResized(object sender, WindowResizedEventArgs e)
        {
            camera.Width = e.width;
            camera.Height = e.height;
        }
    }
}
