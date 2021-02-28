using System;
using static OpenGL.GL;

using System.Numerics;
using System.Drawing;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Diagnostics;

using CrossEngine.Rendering.Geometry;
using CrossEngine.Rendering.Texturing;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Text
{
    class FontAtlasRenderer
    {
        public const string charSet = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        public static readonly int firstChar = (int)charSet[0];
        public static readonly int lastChar = (int)charSet[charSet.Length - 1];

        public static readonly Size textureAtlasSize = new Size(1024, 1024);

        public const float fixedFontSize = 32.0f;

        readonly static System.Drawing.Color atlasForeColor = System.Drawing.Color.White;
        readonly static System.Drawing.Color atlasBackColor = System.Drawing.Color.Black;

        public const int distanceFieldFalloff = 4;
        public const int distanceFieldFalloffDistance = distanceFieldFalloff * 2;

        public static FontAtlas CreateFontAtlas(Font font)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //int middleTime = 0;

            Image fontAtlasImage = new Bitmap(textureAtlasSize.Width, textureAtlasSize.Height, PixelFormat.Format24bppRgb); // create imege
            Graphics drawing = Graphics.FromImage(fontAtlasImage); // create graphics object linked to image

            // preparing drawing and measuring
            drawing.PageUnit = GraphicsUnit.Pixel; // setting unit just in case
            drawing.Clear(atlasBackColor); // painting the back color
            drawing.TextRenderingHint = TextRenderingHint.AntiAlias;
            Brush textBrush = new SolidBrush(atlasForeColor); // creating brush with fore color

            RectangleF[] boundsOfCharacters = new RectangleF[charSet.Length];

            // offsets for drawing characters
            float offsetX = 0;
            float offsetY = 0;
            int characterHeight = font.Height;

            // padding
            SizeF innerPadding = new SizeF(distanceFieldFalloffDistance, distanceFieldFalloffDistance);

            SizeF outerPadding = new SizeF(0, 0);

            // render loop
            for (int index = 0; index < charSet.Length; index++)
            {
                float currentCharacterWidth = drawing.MeasureString(charSet[index].ToString(), font).Width;
                RectangleF totalBox = new RectangleF(offsetX, offsetY,
                    outerPadding.Width*2 + innerPadding.Width*2 + currentCharacterWidth,
                    outerPadding.Height*2 + innerPadding.Height*2 + characterHeight);

                if(totalBox.X + totalBox.Width > textureAtlasSize.Width)
                {
                    offsetX = 0;
                    offsetY += totalBox.Height;
                    totalBox.Location = new PointF(offsetX, offsetY);
                }

                if (totalBox.Y + characterHeight > textureAtlasSize.Height) // this makes it to stop wasting time (nobody needs to draw ouside of the image)
                {
                    Log.Warn("font atlas texture too small!");
                    break;
                }

                PointF characterStart = new PointF(
                    totalBox.X + outerPadding.Width,
                    totalBox.Y + outerPadding.Height);
                SizeF characterSize = new SizeF(
                    totalBox.Width - outerPadding.Width * 2,
                    totalBox.Height - outerPadding.Height * 2);

                RectangleF currentCharacter = new RectangleF(characterStart, characterSize);
                boundsOfCharacters[index] = currentCharacter;

                PointF drawingStart = new PointF(characterStart.X + innerPadding.Width, characterStart.Y + innerPadding.Height);
                drawing.DrawString(charSet[index].ToString(), font, textBrush, drawingStart);

                offsetX += totalBox.Width;
            }

            NormalizeRectangles(boundsOfCharacters, textureAtlasSize, out Vector2 pixelSize);

            //middleTime = (int)stopwatch.ElapsedMilliseconds;
            //Log.Debug("generated font atlas in: " + middleTime + " ms");

            // generate distance field, crop
            Size distanceFieldSize = new Size(textureAtlasSize.Width / 2, textureAtlasSize.Height / 2);
            Image distanceFieldImage = ((Bitmap)DistanceFieldGenerator.Generate(fontAtlasImage, distanceFieldFalloff)).Clone(new Rectangle(distanceFieldFalloffDistance, distanceFieldFalloffDistance, distanceFieldSize.Width, distanceFieldSize.Height), PixelFormat.Format8bppIndexed);

            Texture fontAtlasTexture = new Texture(distanceFieldImage);
            FontAtlas fontStruct = new FontAtlas(fontAtlasTexture, font, boundsOfCharacters);
            fontStruct.pixelSize = pixelSize;

            //Log.Debug("generated distance field in: " + ((int)stopwatch.ElapsedMilliseconds - middleTime) + " ms");

            // cleanup
            textBrush.Dispose();
            drawing.Dispose();
            fontAtlasImage.Dispose();
            distanceFieldImage.Dispose();

            //stopwatch.Stop();
            //Log.Debug("finished in: " + (int)stopwatch.ElapsedMilliseconds + " ms");

            return fontStruct;
        }

        static void NormalizeRectangles(RectangleF[] rects, Size textureSize, out Vector2 pixelSize)
        {
            SizeF pixelMultiplier = new SizeF(1.0f / textureSize.Width, 1.0f / textureSize.Height);
            pixelSize = Vector2.One / Vector2Extension.FromSizeF(pixelMultiplier);
            for (int i = 0; i < rects.Length; i++)
            {
                rects[i].X *= pixelMultiplier.Width;
                rects[i].Y *= pixelMultiplier.Height;
                rects[i].Width *= pixelMultiplier.Width;
                rects[i].Height *= pixelMultiplier.Height;
            }
        }

        //####################################################################################################

        // used for simple static text
        //public static Image DrawText(string text, Font font, System.Drawing.Color textColor, System.Drawing.Color backColor, TextRenderingHint renderingHint = TextRenderingHint.AntiAlias)
        //{
        //    DateTime start = DateTime.Now;
        //
        //    // dummy bitmap just to get a graphics object
        //    Image img = new Bitmap(1, 1);
        //    Graphics drawing = Graphics.FromImage(img);
        //
        //    SizeF textSize = drawing.MeasureString(text, font);
        //
        //    // cleanup of dummy image (drawing needs to be cleaned up as well or it would draw into the old image)
        //    img.Dispose();
        //    drawing.Dispose();
        //
        //    // new image of the right size
        //    img = new Bitmap((int)textSize.Width, (int)textSize.Height);
        //
        //    drawing = Graphics.FromImage(img);
        //
        //    // background color
        //    drawing.Clear(backColor);
        //
        //    // (text brush)
        //    Brush textBrush = new SolidBrush(textColor);
        //
        //    drawing.TextRenderingHint = renderingHint;
        //    drawing.DrawString(text, font, textBrush, 0, 0);
        //
        //    drawing.Save();
        //
        //    textBrush.Dispose();
        //    drawing.Dispose();
        //
        //    DateTime end = DateTime.Now;
        //    Log.Debug("generated text texture in: " + Convert.ToString((end - start).TotalMilliseconds) + " ms");
        //
        //    return img;
        //}

        /*
        public static void ShowRectArray(RectangleF[] rects)
        {
            foreach(RectangleF rect in rects)
            {
                Log.Debug(rect.X.ToString() + rect.Y.ToString() + rect.Width.ToString() + rect.Height.ToString());
            }
        }
        */
    }
}
