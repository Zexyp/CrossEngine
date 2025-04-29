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
    public interface IOverlay
    {
        public abstract void Resize(float width, float height);
        public abstract void Draw();
    }

    public interface ISceneOverlay : IOverlay
    {
        public ICamera Camera { get; set; }
    }

    public abstract class HudOverlay : IOverlay
    {
        protected readonly Camera Camera = new Camera();
        protected Vector2 Size;

        public virtual void Resize(float width, float height)
        {
            Size = new Vector2(width, height);

            Camera.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
        }

        public virtual void Draw()
        {
            Renderer2D.BeginScene(((ICamera)Camera).GetViewProjectionMatrix());
            LineRenderer.BeginScene(((ICamera)Camera).GetViewProjectionMatrix());

            var prevTextMode = TextRendererUtil.SetMode(TextRendererUtil.DrawMode.YDown);

            Content();

            TextRendererUtil.SetMode(prevTextMode);

            LineRenderer.EndScene();
            Renderer2D.EndScene();
        }

        protected virtual void Content() { }
    }
}
