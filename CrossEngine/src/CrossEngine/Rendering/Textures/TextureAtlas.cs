using System;

using System.Drawing;
using System.Numerics;
using System.Diagnostics;

using CrossEngine.Utils;

namespace CrossEngine.Rendering.Textures
{
    public class TextureAtlas
    {
        public Vector4 Margin { get; private init; }
        public int NumTiles { get; private init; }

        Vector4[] spriteOffsets;
        
        public TextureAtlas(Vector2 sheetSize, Vector2 spriteSize, int numTiles, Vector4 margin = default)
        {
            NumTiles = numTiles;
            Margin = margin;

            this.spriteOffsets = new Vector4[NumTiles];

            var x = 2;
            var y = 3;
            var sheetWidth = sheetSize.X;
            var sheetHeight = sheetSize.Y;
            var spriteWidth = spriteSize.X;
            var spriteHeight = spriteSize.Y;

            float offx = Margin.X / sheetWidth;
            float offy = (Margin.Y + Margin.W) / sheetHeight;
            for (int i = 0; i < NumTiles; i++)
            {
                spriteOffsets[i] = new Vector4(offx, -offy, (spriteWidth) / sheetWidth, (spriteHeight) / sheetHeight);
                spriteOffsets[i].Y -= spriteOffsets[i].W;
                offx += (spriteWidth + Margin.X + Margin.Z) / sheetWidth;
                if (offx >= 1)
                {
                    offx = Margin.X / sheetWidth;
                    offy += (spriteHeight + Margin.Y + Margin.W) / sheetHeight;
                }
            }
        }

        public Vector4 GetTextureOffsets(int index)
        {
            return spriteOffsets[index];
        }
    }
}
