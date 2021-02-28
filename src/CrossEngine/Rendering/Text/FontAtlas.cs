using System;

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Numerics;

using CrossEngine.Rendering.Texturing;

namespace CrossEngine.Rendering.Text
{
    // structure holding font atlas data
    public class FontAtlas
    {
        public Texture texture;
        public Font font;
        public RectangleF[] characters;
        public Vector2 pixelSize;

        public FontAtlas(Texture texture, Font font, RectangleF[] characters)
        {
            this.texture = texture;
            this.font = font;
            this.characters = characters;
        }
    }
}
