using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

using CrossEngine.Logging;
using CrossEngine.Rendering;
using CrossEngine.Rendering.Textures;
using CrossEngine.Platform.OpenGL;
using StbImageSharp;
using CrossEngine.Utils;

namespace CrossEngine.Loaders
{
    // task are ass, just return a container and fill it later
    public static class TextureLoader
    {
        public static WeakReference<Texture> DefaultTexture { get; private set; }

        // wtf, how is this not crashing the whole thing?
        // (the static ctor is very funky - nobody knows when or how it gets called
        // however we know it's before the first call to any member
        // and i can say for certain that it's even on the same thread)
        //static TextureLoader()
        //{
        //    
        //}

        [ThreadStatic]
        internal static Action<Action> ServiceRequest;

        internal static unsafe void Init()
        {
            //DefaultTexture = Texture.Create(1, 1, ColorFormat.RGBA);
            //uint col = 0xffff00ff;
            //DefaultTexture.GetValue().SetData(&col, sizeof(uint));
            DefaultTexture = LoadTextureFromBytes(Properties.Resources.DefaultTexture);
        
            var gapi = RendererApi.GetApi();

            Debug.Assert(gapi == GraphicsApi.OpenGL || gapi == GraphicsApi.OpenGLES, "only OpenGL supported");
            StbImage.stbi_set_flip_vertically_on_load(1);
        }

        internal static void Shutdown()
        {
            DefaultTexture.Dispose();
            DefaultTexture = null;
        }

        public static WeakReference<Texture> LoadTextureFromFile(string filepath, ColorFormat? desiredFormat = null)
        {
            using (Stream stream = File.OpenRead(filepath))
            {
                return LoadTextureFromStream(stream, desiredFormat);
            }
        }

        public static WeakReference<Texture> LoadTextureFromBytes(byte[] filedata, ColorFormat? desiredFormat = null)
        {
            ImageResult result = ImageResult.FromMemory(filedata);
            return InternalLoad(result, desiredFormat);
        }

        public static WeakReference<Texture> LoadTextureFromStream(Stream filedata, ColorFormat? desiredFormat = null)
        {
            ImageResult result = ImageResult.FromStream(filedata);
            return InternalLoad(result, desiredFormat);
        }

        private static unsafe WeakReference<Texture> InternalLoad(ImageResult result, ColorFormat? desiredFormat = null)
        {
            var gapi = RendererApi.GetApi();

            var format = ColorComponentsToColorFormat(result.Comp);
            ColorFormat desired = desiredFormat ?? format;

            WeakReference<Texture> texture = new WeakReference<Texture>(null);

            ServiceRequest.Invoke(() =>
            {
                Texture.Create(texture, (uint)result.Width, (uint)result.Height, format);
                fixed (void* p = result.Data)
                    ((GLTexture)texture.GetValue()).SetData(p, (uint)result.Width, (uint)result.Height, format, desired);
            });

            return texture;
        }

        private static ColorFormat ColorComponentsToColorFormat(ColorComponents components)
        {
            switch (components)
            {
                case ColorComponents.RedGreenBlue:
                    return ColorFormat.RGB;
                case ColorComponents.RedGreenBlueAlpha:
                    return ColorFormat.RGBA;
                case ColorComponents.Default:
                case ColorComponents.Grey:
                case ColorComponents.GreyAlpha:
                    throw new NotSupportedException();
                default:
                    Debug.Assert(false, "unknow type");
                    return ColorFormat.None;
            }
        }
    }
}