using System;

using System.Drawing;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Utils;

namespace CrossEngine.Rendering.Textures
{
    public class TextureAtlas
    {
        RectangleF[] tiles;
        float tileWidth = 0;
        float tileHeight = 0;
        
        public TextureAtlas(Vector2 size, float tileWidth, float tileHeight, int numTiles, float spacing = 0)
        {
            this.tiles = new RectangleF[numTiles];

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;

            float currentX = 0;
            float currentY = 0;
            for (int i = 0; i < numTiles; i++)
            {
                RectangleF rect = new RectangleF(currentX / size.X,
                                                 currentY / size.Y,
                                                 this.tileWidth / size.X,
                                                 this.tileHeight / size.Y);

                tiles[i] = rect;

                currentX += this.tileWidth + spacing;
                if (currentX >= size.X)
                {
                    currentX = 0;
                    currentY += this.tileHeight + spacing;
                }

                Debug.Assert(currentY < size.Y);
            }
        }

        public Vector4 GetTextureOffsets(int index)
        {
            RectangleF rect = tiles[index];
            return new Vector4(rect.X, 1 - rect.Y, rect.Width, rect.Height);
        }
    }
}
