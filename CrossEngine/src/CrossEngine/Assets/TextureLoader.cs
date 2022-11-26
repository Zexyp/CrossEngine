using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.Platform.OpenGL;
using CrossEngine.Utils.Imaging;

namespace CrossEngine.Assets
{
    public static class TextureLoader
    {
        // wtf, how is this not crashing the whole thing? (needs to be invoked on a specific thread)
        public static readonly Ref<Texture> DefaultTexture;

        static unsafe TextureLoader()
        {
            DefaultTexture = Texture.Create(1, 1, ColorFormat.RGBA);
            uint col = 0xffff00ff;
            DefaultTexture.Value.SetData(&col, (uint)sizeof(uint));
        }


        // able to load BMP, GIF, EXIF, JPG, PNG, TIFF (, EMF)
        public static Ref<Texture> LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Log.Default.Error($"texture not found ('{path}')");
                return DefaultTexture;
            }

            Bitmap image = null;
            try
            {
                image = new Bitmap(path);
            }
            catch (ArgumentException ex)
            {
                Log.Default.Error($"invalid image format ('{path}'): {ex.Message}");
                return DefaultTexture;
            }

            var texture = LoadTexture(image);
            
            // free up resources
            image.Dispose();

            return texture;
        }

        public static unsafe Ref<Texture> LoadTexture(Bitmap image)
        {
            //if (RendererAPI.GetAPI() == RendererAPI.API.OpenGL)
            //{
            //    // cuz OpenGL
            //    image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            //}

            ColorFormat format = 0;
            ColorFormat desired = 0;

            Bitmap converted = null;
            switch (image.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    {
                        format = ColorFormat.BGRA;
                        desired = ColorFormat.RGBA;
                    }
                    break;
                case PixelFormat.Format24bppRgb:
                    {
                        format = ColorFormat.BGR;
                        desired = ColorFormat.RGB;
                    }
                    break;
                //case PixelFormat.Format8bppIndexed:
                //    {
                //        format = ColorFormat.SingleR;
                //        desired = ColorFormat.SingleR;
                //    }
                //    break;
                default:
                    {
                        //Debug.Assert(false, $"{nameof(PixelFormat)} of image not supported");

                        if (Image.IsAlphaPixelFormat(image.PixelFormat))
                        {
                            converted = new Bitmap(ImageUtils.CreateNewWithPixelFormat(image, PixelFormat.Format32bppArgb));

                            format = ColorFormat.BGRA;
                            desired = ColorFormat.RGBA;
                        }
                        else
                        {
                            converted = (Bitmap)ImageUtils.CreateNewWithPixelFormat(image, PixelFormat.Format24bppRgb);

                            format = ColorFormat.BGR;
                            desired = ColorFormat.RGB;
                        }

                        Log.Default.Trace($"image format converted (from '{image.PixelFormat}' to '{converted.PixelFormat}')");
                    }
                    break;
            }

            var texture = converted == null ?
                          UploadTextureFromImage(image, format, desired) :
                          UploadTextureFromImage(converted, format, desired);

            converted?.Dispose();

            return texture;
        }

        private unsafe static Ref<Texture> UploadTextureFromImage(Bitmap image, ColorFormat format, ColorFormat desired)
        {
            Debug.Assert(RendererAPI.GetAPI() == RendererAPI.API.OpenGL);

            bool glflip = RendererAPI.GetAPI() == RendererAPI.API.OpenGL;
            if (glflip) image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            BitmapData imageData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

            var texture = Texture.Create((uint)imageData.Width, (uint)imageData.Height, desired);
            ((GLTexture)texture.Value).SetData(imageData.Scan0.ToPointer(), (uint)imageData.Width, (uint)imageData.Height, format, desired);

            image.UnlockBits(imageData);

            if (glflip) image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return texture;
        }
    }
}
