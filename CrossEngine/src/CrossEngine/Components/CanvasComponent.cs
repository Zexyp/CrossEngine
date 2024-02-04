using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CrossEngine.Ecs;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Rendering.Renderables;

namespace CrossEngine.Components
{
    public class CanvasComponent : Component, IObjectRenderData
    {
        public Vector2 Size { get; private set; }

        Matrix4x4 IObjectRenderData.Transform => throw new NotImplementedException();

        internal Camera camera = new Camera();

        public void Resize(float width, float height)
        {
            Size = new Vector2(width, height);
            camera.SetOrtho(width, height);
        }
    }
}
