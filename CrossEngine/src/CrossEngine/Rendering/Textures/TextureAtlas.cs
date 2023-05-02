using System;

using System.Drawing;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Utils;

namespace CrossEngine.Rendering.Textures
{
    public class TextureAtlas
    {
        Vector4[] sprites;

        
        public TextureAtlas(Vector2 sheetSize, Vector2 spriteSize, int numTiles)
        {
            this.sprites = new Vector4[numTiles];

            var x = 2;
            var y = 3;
            var sheetWidth = sheetSize.X;
            var sheetHeight = sheetSize.Y;
            var spriteWidth = spriteSize.X;
            var spriteHeight = spriteSize.Y;

            float offx = 0;
            float offy = 0;
            for (int i = 0; i < numTiles; i++)
            {
                sprites[i] = new Vector4(offx, -offy, spriteWidth / sheetWidth, spriteHeight / sheetHeight);
                sprites[i].Y -= sprites[i].W;
                offx += spriteWidth / sheetWidth;
                if (offx >= 1)
                {
                    offx = 0;
                    offy += spriteHeight / sheetHeight;
                }
            }
        }

        public Vector4 GetTextureOffsets(int index)
        {
            return sprites[index];
        }
    }
}
