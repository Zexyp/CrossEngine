using CrossEngine.Components;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Ecs.Components
{
    internal class OrthographicCameraComponent : CameraComponent
    {
        float Far = 1;
        float Near = -1;
        float Scale = 1;

        public override void Resize(float width, float height)
        {
            float aspect = width / height;
            // rip depth
            // TODO: fix
            ProjectionMatrix = Matrix4x4.CreateOrthographic(aspect * Scale, Scale, 0, Far);
        }
    }
}
