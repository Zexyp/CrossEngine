using CrossEngine.Components;
using CrossEngine.Rendering;
using System.Numerics;

namespace CrossEngine.Components
{
    public class RectangleComponent : UIComponent
    {
        public Vector4 Color = Vector4.One;
        public Vector2 Size = Vector2.One;

        protected internal override void Draw()
        {
            Renderer2D.DrawQuad(Matrix4x4.CreateScale(new Vector3(Size, 1)) * Entity.Transform.TransformMatrix, Color);
        }
    }
}
