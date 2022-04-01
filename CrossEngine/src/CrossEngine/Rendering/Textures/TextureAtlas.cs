using System;

using System.Drawing;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Utils;

namespace CrossEngine.Rendering.Textures
{
    public class TextureAtlas
    {
        public Ref<Texture> Texture { get; private set; }
        RectangleF[] tiles;
        float tileWidth = 0;
        float tileHeight = 0;
        
        public TextureAtlas(Ref<Texture> texture, float tileWidth, float tileHeight, int numTiles, float spacing = 0)
        {
            this.Texture = texture;

            this.tiles = new RectangleF[numTiles];

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;

            float currentX = 0;
            float currentY = 0;
            for (int i = 0; i < numTiles; i++)
            {
                RectangleF rect = new RectangleF(currentX / (float)this.Texture.Value.Width,
                                                 currentY / (float)this.Texture.Value.Height,
                                                 this.tileWidth / (float)this.Texture.Value.Width,
                                                 this.tileHeight / (float)this.Texture.Value.Height);

                tiles[i] = rect;

                currentX += this.tileWidth + spacing;
                if (currentX >= Texture.Value.Width)
                {
                    currentX = 0;
                    currentY += this.tileHeight + spacing;
                }

                Debug.Assert(currentY < texture.Value.Height);
            }
        }

        public Vector4 GetTextureOffsets(int index)
        {
            RectangleF rect = tiles[index];
            return new Vector4(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
