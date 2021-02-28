using System;

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CrossEngine.Rendering.Text
{
    class DistanceFieldGenerator // v 4.1
    {
        public static Image Generate(Image image, int falloff)
        {
            // falloff 1 -> 2px fallof
            falloff *= 4; // even hotter fix
            image = ConvertToDistanceField(image, falloff);
            image = DownsampleLuminanceBy2(image);
            return image;
        }

        static Image ConvertToDistanceField(Image image, int falloff)
        {
            Bitmap source = (Bitmap)image;
            int oldWidth = source.Width;
            int oldHeight = source.Height;
            int newWidth = oldWidth + 2 * falloff;
            int newHeight = oldHeight + 2 * falloff;
            float[] ping = new float[newWidth * newHeight << 2];
            float[] pong = new float[newWidth * newHeight << 2];

            for (int y = 0, i = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++, i += 4)
                {
                    if (falloff <= x && x < falloff + oldWidth &&
                        falloff <= y && y < falloff + oldHeight &&
                        source.GetPixel(x - falloff, y - falloff).R > 127)
                    {
                        ping[i + 0] = x;
                        ping[i + 1] = y;
                        ping[i + 2] = float.NaN;
                        ping[i + 3] = float.NaN;
                    }
                    else
                    {
                        ping[i + 0] = float.NaN;
                        ping[i + 1] = float.NaN;
                        ping[i + 2] = x;
                        ping[i + 3] = y;
                    }
                }
            }

            var step = 1;
            while (step < newWidth || step < newHeight)
            {
                step <<= 1;
            }

            while (step > 0)
            {
                for (int y = 0, i = 0; y < newHeight; y++)
                {
                    for (int x = 0; x < newWidth; x++, i += 4)
                    {
                        float bestFirstDistance = float.PositiveInfinity;
                        float bestFirstX = float.NaN;
                        float bestFirstY = float.NaN;

                        float bestSecondDistance = float.PositiveInfinity;
                        float bestSecondX = float.NaN;
                        float bestSecondY = float.NaN;

                        for (var neighbor = 0; neighbor < 9; neighbor++)
                        {
                            int nx = x + (neighbor % 3 - 1) * step;
                            int ny = y + ((neighbor / 3 | 0) - 1) * step;

                            if (0 <= nx && nx < newWidth && 0 <= ny && ny < newHeight)
                            {
                                int j = nx + ny * newWidth << 2;
                                float oldBestFirstX = ping[j + 0];
                                float oldBestFirstY = ping[j + 1];
                                float oldBestSecondX = ping[j + 2];
                                float oldBestSecondY = ping[j + 3];

                                {
                                    float dx = x - oldBestFirstX;
                                    float dy = y - oldBestFirstY;
                                    float d = dx * dx + dy * dy;

                                    if (d < bestFirstDistance)
                                    {
                                        bestFirstX = oldBestFirstX;
                                        bestFirstY = oldBestFirstY;
                                        bestFirstDistance = d;
                                    }
                                }

                                {
                                    float dx = x - oldBestSecondX;
                                    float dy = y - oldBestSecondY;
                                    float d = dx * dx + dy * dy;

                                    if (d < bestSecondDistance)
                                    {
                                        bestSecondX = oldBestSecondX;
                                        bestSecondY = oldBestSecondY;
                                        bestSecondDistance = d;
                                    }
                                }
                            }
                        }

                        pong[i + 0] = bestFirstX;
                        pong[i + 1] = bestFirstY;
                        pong[i + 2] = bestSecondX;
                        pong[i + 3] = bestSecondY;
                    }
                }

                float[] swap = ping;
                ping = pong;
                pong = swap;
                step >>= 1;
            }

            byte[] bytes = new byte[newWidth * newHeight/* << 2*/];

            // Merge the two distance transforms together to get an RGBA signed distance field
            for (int y = 0, i = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++, i += 4)
                {
                    float firstX = ping[i + 0] - x;
                    float firstY = ping[i + 1] - y;
                    double firstD = (float)Math.Sqrt(firstX * firstX + firstY * firstY);

                    float secondX = ping[i + 2] - x;
                    float secondY = ping[i + 3] - y;
                    double secondD = (float)Math.Sqrt(secondX * secondX + secondY * secondY);

                    bytes[i >> 2] = /*bytes[i + 1] = bytes[i + 2] = */(byte)(int)(false ? 0 : firstD > secondD ? Math.Max(0, Math.Round(255 * (0.5 - 0.5 * (firstD - 0.5) / (falloff + 0.5)))) : Math.Min(255, Math.Round(255 * (0.5 + 0.5 * (secondD - 0.5) / (falloff + 0.5)))));
                    //bytes[i + 3] = 255;
                }
            }

            return CreateImage(bytes, newWidth, newHeight, true);
        }

        static Image DownsampleLuminanceBy2(Image image)
        {
            Bitmap source = (Bitmap)image;
            int oldWidth = source.Width;
            int oldHeight = source.Height;
            int newWidth = oldWidth >> 1;
            int newHeight = oldHeight >> 1;
            byte[] newData = new byte[newWidth * newHeight];
            int oldStride = oldWidth;

            for (var y = 0; y < newHeight; y++)
            {
                int from = (y << 1) * oldStride;
                int to = y * newWidth;

                for (var x = 0; x < newWidth; x++)
                {
                    newData[to] = (byte)(int)((
                      source.GetPixel(from % oldWidth, from / oldWidth).R +
                      source.GetPixel((from + 1) % oldWidth, (from + 1) / oldWidth).R +
                      source.GetPixel((from + oldStride) % oldWidth, (from + oldStride) / oldWidth).R +
                      source.GetPixel((from + oldStride + 1) % oldWidth, (from + oldStride + 1) / oldWidth).R
                    ) >> 2);
                    from += 2;
                    to++;
                }
            }

            return CreateImage(newData, newWidth, newHeight, true);
        }

        static unsafe Image CreateImage(byte[] imageData, int width, int height, bool plsPalette = false)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            Marshal.Copy(imageData, 0, data.Scan0, imageData.Length);

            bitmap.UnlockBits(data);

            if (plsPalette)
            {
                ColorPalette pal = bitmap.Palette;
                for (int i = 0; i <= 255; i++)
                {
                    // create greyscale color table
                    pal.Entries[i] = Color.FromArgb(i, i, i);
                }
                bitmap.Palette = pal;
            }

            return bitmap;
        }
    }
}
