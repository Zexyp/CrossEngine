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
    internal class PerspectiveCameraComponent : CameraComponent
    {
        float FOV = 90;
        float Near = .1f;
        float Far = 100;

        public override void Resize(float width, float height)
        {
            float aspect = width / height;
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(FOV, aspect, Near, Far);
        }
    }
}
