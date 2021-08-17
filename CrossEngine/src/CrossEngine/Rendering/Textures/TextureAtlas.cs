using System;

using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

using CrossEngine.Rendering.Sprites;
using CrossEngine.Rendering.Textures;

namespace CrossEngine.Rendering.Textures
{
    public class TextureAtlas
    {
        public Texture Texture { get; private set; }
        RectangleF[] tiles;
        Sprite[] sprites;
        int tileWidth = 0;
        int tileHeight = 0;

        public TextureAtlas(Texture texture, int tileWidth, int tileHeight, int numTiles, int spacing = 0, bool prepareSprites = true)
        {
            this.Texture = texture;

            this.tiles = new RectangleF[numTiles];

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;

            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < numTiles; i++)
            {
                RectangleF rect = new RectangleF(currentX / (float)this.Texture.Width, currentY / (float)this.Texture.Height, this.tileWidth / (float)this.Texture.Width, this.tileHeight / (float)this.Texture.Height);

                tiles[i] = rect;

                currentX += this.tileWidth + spacing;
                if (currentX >= texture.Width)
                {
                    currentX = 0;
                    currentY += this.tileHeight + spacing;
                }
            }

            if (prepareSprites)
                PrepareSprites();
        }

        public void PrepareSprites()
        {
            if (sprites != null)
                return;

            sprites = new Sprite[tiles.Length];
            for (int i = 0; i < tiles.Length; i++)
            {
                RectangleF rect = tiles[i];
                Vector2[] texCoords = {
                        new Vector2(rect.X,     rect.Y),
                        new Vector2(rect.Right, rect.Y),
                        new Vector2(rect.Right, rect.Bottom),
                        new Vector2(rect.X,     rect.Bottom)
                };

                Sprite sprite = new Sprite(this.Texture, texCoords, new Vector2(tileWidth, tileHeight));
                sprites[i] = sprite;
            }
        }

        public Sprite GetSprite(int index)
        {
            if (sprites != null)
                return sprites[index];
            else
                return null;
        }

        public Vector4 GetTextureOffsets(int index)
        {
            RectangleF rect = tiles[index];
            return new Vector4(rect.X, rect.Y, 1.0f / rect.Width, 1.0f / rect.Height);
        }
    }
}
