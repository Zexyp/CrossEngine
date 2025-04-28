using System;

using System.Drawing;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Utils;

namespace CrossEngine.Rendering.Textures
{
    public static class TextureAtlas
    {
        // margin: left, top, right, bottom
        // offsets: offx, offy, scalex, scaley
        public static Vector4[] CreateOffsets(Vector2 sheetSize, Vector2 spriteSize, int numTiles, Vector4 margin = default)
        {
            Vector4[] spriteOffsets = new Vector4[numTiles];
            
            var sheetWidth = sheetSize.X;
            var sheetHeight = sheetSize.Y;
            var spriteWidth = spriteSize.X;
            var spriteHeight = spriteSize.Y;

            float offx = margin.X / sheetWidth;
            float offy = (margin.Y + margin.W) / sheetHeight;
            for (int i = 0; i < numTiles; i++)
            {
                spriteOffsets[i] = new Vector4(offx, -offy, (spriteWidth) / sheetWidth, (spriteHeight) / sheetHeight);
                spriteOffsets[i].Y -= spriteOffsets[i].W;
                offx += (spriteWidth + margin.X + margin.Z) / sheetWidth;
                if (offx >= 1)
                {
                    offx = margin.X / sheetWidth;
                    offy += (spriteHeight + margin.Y + margin.W) / sheetHeight;
                }
            }
            
            return spriteOffsets;
        }
    }
}
