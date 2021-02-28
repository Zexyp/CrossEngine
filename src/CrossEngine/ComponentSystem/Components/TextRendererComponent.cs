using System.Numerics;

using CrossEngine.Rendering.Text;

namespace CrossEngine.ComponentSystem.Components
{
    class TextRendererComponent : Component
    {
        public TextProperties TextProperties { get; set; }
        public string text = "";

        public override void OnRender()
        {
            TextRenderer.DrawText(text, TextProperties, entity.transform.Position, Vector2.One, entity.transform.Rotation);
        }
    }
}
