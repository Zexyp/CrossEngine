using CrossEngine.Rendering;
using CrossEngine.Rendering.Cameras;
using CrossEngine.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Debugging
{
    public abstract class Overlay
    {
        public readonly Camera Camera = new Camera();

        protected Vector2 Size;

        public virtual void Resize(float width, float height)
        {
            Size = new Vector2(width, height);
            Camera.ProjectionMatrix = Matrix4x4.CreateOrthographic(width, height, -1, 1);
        }

        public virtual void Draw()
        {
            Renderer2D.BeginScene(Camera.ViewProjectionMatrix);
            LineRenderer.BeginScene(Camera.ViewProjectionMatrix);
            Content();
            LineRenderer.EndScene();
            Renderer2D.EndScene();
        }

        protected abstract void Content();
    }
}
