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
using CrossEngine.Profiling;
using CrossEngine.Platform;
using System.Threading.Tasks;

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
        internal static Func<Action, Task> ServiceRequest;

        internal static unsafe void Init()
        {
            var gapi = RendererApi.GetApi();
            if (gapi == GraphicsApi.OpenGL || gapi == GraphicsApi.OpenGLES)
                StbImage.stbi_set_flip_vertically_on_load(1);

            //DefaultTexture = Texture.Create(1, 1, ColorFormat.RGBA);
            //uint col = 0xffff00ff;
            //DefaultTexture.GetValue().SetData(&col, sizeof(uint));

            DefaultTexture = LoadTextureFromBytes(Properties.Resources.DefaultTexture);
        
        }

        internal static void Shutdown()
        {
            DefaultTexture.Dispose();
            DefaultTexture = null;
        }

        public static async Task<WeakReference<Texture>> LoadTextureFromFile(string filepath, ColorFormat? desiredFormat = null)
        {
            using (Stream stream = await PlatformHelper.FileRead(filepath))
            {
                return LoadTextureFromStream(stream, desiredFormat);
            }
        }

        public static WeakReference<Texture> LoadTextureFromBytes(byte[] filedata, ColorFormat? desiredFormat = null)
        {
            Profiler.BeginScope("texture parsing");
            ImageResult result = ImageResult.FromMemory(filedata);
            Profiler.EndScope();
            return InternalLoad(result, desiredFormat);
        }

        public static WeakReference<Texture> LoadTextureFromStream(Stream filedata, ColorFormat? desiredFormat = null)
        {
            Profiler.BeginScope("texture parsing");
            ImageResult result = ImageResult.FromStream(filedata);
            Profiler.EndScope();
            return InternalLoad(result, desiredFormat);
        }

        public static void Free(WeakReference<Texture> texture)
        {
            ServiceRequest.Invoke(texture.Dispose);
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
                {
                    if (RendererApi.GetApi() == GraphicsApi.OpenGL)
                        ((GLTexture)texture.GetValue()).SetData(p, (uint)result.Width, (uint)result.Height, format, desired);
                    else
                        texture.GetValue().SetData(p, (uint)(result.Width * result.Height * GetPixelSize(result.Comp)));
                }
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

        private static uint GetPixelSize(ColorComponents comp)
        {
            switch (comp)
            {
                case ColorComponents.RedGreenBlue:
                    return 3;
                case ColorComponents.RedGreenBlueAlpha:
                    return 4;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}