using static OpenGL.GL;

using System.Numerics;

using CrossEngine.Rendering.Texturing;

namespace CrossEngine.Rendering.Sprites
{
    public class Sprite
    {
        public Texture texture;
        public Vector2[] texCoords;
        public Vector2 size;

        public Sprite(Texture texture)
        {
            this.texture = texture;

            Vector2[] texCoords = {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };
            this.texCoords = texCoords;

            size = new Vector2(this.texture.width, this.texture.height);
        }

        public Sprite(Texture texture, Vector2[] texCoords, Vector2 size)
        {
            this.texture = texture;
            this.texCoords = texCoords;
            this.size = size;
        }

        public Sprite FilterParameter(FilterParameter parameter)
        {
            texture.SetFilterParameter(parameter);
            return this;
        }
    }
}
