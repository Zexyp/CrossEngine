using System;

using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using CrossEngine.Rendering.Texturing;

namespace CrossEngine.Rendering.Sprites
{
    class SpriteSheet
    {
        Texture texture;
        List<Sprite> sprites;

        public SpriteSheet(Texture texture, int spriteWidth, int spriteHeight, int numSprites, int spacing = 0)
        {
            this.sprites = new List<Sprite> { };
            this.texture = texture;

            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < numSprites; i++)
            {
                RectangleF rect = new RectangleF(currentX / (float)texture.width, currentY / (float)texture.height, spriteWidth / (float)texture.width, spriteHeight / (float)texture.height);

                Vector2[] texCoords = {
                    new Vector2(rect.X,     rect.Y),
                    new Vector2(rect.Right, rect.Y),
                    new Vector2(rect.Right, rect.Bottom),
                    new Vector2(rect.X,     rect.Bottom)
                };

                Sprite sprite = new Sprite(this.texture, texCoords, new Vector2(spriteWidth, spriteHeight));
                sprites.Add(sprite);

                currentX += spriteWidth + spacing;
                if (currentX >= texture.width)
                {
                    currentX = 0;
                    currentY += spriteHeight + spacing;
                }
            }
        }

        public Sprite GetSprite(int index)
        {
            return sprites[index];
        }

        public SpriteSheet FilterParameter(FilterParameter parameter)
        {
            texture.SetFilterParameter(parameter);
            return this;
        }
    }
}
