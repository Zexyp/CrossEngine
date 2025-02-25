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
        
        protected Vector2 LeftBottom;
        protected Vector2 LeftTop;
        protected Vector2 RightBottom;
        protected Vector2 RightTop;
        protected Vector2 Center;

        public virtual void Resize(float width, float height)
        {
            Size = new Vector2(width, height);
            
            LeftTop = Vector2.Zero;
            LeftBottom = new Vector2(0, Size.Y);
            RightBottom = new Vector2(Size.X, Size.Y);
            RightTop = new Vector2(Size.X, 0);
            Center = new Vector2(Size.X / 2, Size.Y / 2);
            
            Camera.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
        }

        public virtual void Draw()
        {
            Renderer2D.BeginScene(Camera.ViewProjectionMatrix);
            LineRenderer.BeginScene(Camera.ViewProjectionMatrix);
            
            var prevTextMode = TextRendererUtil.Mode;
            TextRendererUtil.Mode = TextRendererUtil.DrawMode.YDown;
            
            Content();
            
            TextRendererUtil.Mode = prevTextMode;
            
            LineRenderer.EndScene();
            Renderer2D.EndScene();
        }

        protected virtual void Content() { }
    }
}
