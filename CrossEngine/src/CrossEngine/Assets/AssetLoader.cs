using System;

using System.Drawing;
using System.IO;

using CrossEngine.Logging;
using CrossEngine.Rendering.Textures;

namespace CrossEngine.Assets
{
    public static class AssetLoader
    {
        // wtf, how is this not crashing the whole thing?
        public static readonly Texture DefaultTexture = new Texture(0xff00ff);

        // able to load BMP, GIF, EXIF, JPG, PNG, TIFF (, EMF)
        public static Texture LoadTexture(string path)
        {
            if (!File.Exists(path))
            {
                Log.Core.Error($"texture not found ('{path}')");
                return DefaultTexture;
            }

            Bitmap image = null;
            try
            {
                image = new Bitmap(path);
            }
            catch (ArgumentException ex)
            {
                Log.Core.Error($"invalid image format ('{path}'): {ex.Message}");
                return DefaultTexture;
            }

            // cuz OpenGL
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Texture texture = new Texture(image);

            // free up resources
            image.Dispose();

            return texture;
        }
    }
}
