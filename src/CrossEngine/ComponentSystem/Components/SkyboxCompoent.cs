using System;

using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;

namespace CrossEngine.ComponentSystem.Components
{
    public class SkyboxCompoent : Component
    {
        Skybox skybox;

        public SkyboxCompoent(Skybox skybox)
        {
            this.skybox = skybox;
        }

        public override void OnRender()
        {
            skybox.Draw(ActiveCamera.camera);
        }
    }
}
