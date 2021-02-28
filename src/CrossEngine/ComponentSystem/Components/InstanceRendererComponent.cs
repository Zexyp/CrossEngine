using System;

using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering;

namespace CrossEngine.ComponentSystem.Components
{
    public class InstanceRendererComponent : Component
    {
        Mesh mesh;

        public InstanceRendererComponent(Mesh mesh)
        {
            this.mesh = mesh;
            InstanceRenderer.Register(this.mesh, this);
        }
    }
}
