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

namespace CrossEngine.Assets.Loaders
{
    public static class TextureLoader
    {
        // TODO: shutdown

        // wtf, how is this not crashing the whole thing?
        // (the static ctor is very funky - nobody knows when or how it gets called
        // however we know it's before the first call to any member
        // and i can say for certain that it's even on the same thread)
        public static readonly WeakReference<Texture> DefaultTexture;

        static unsafe TextureLoader()
        {
            // me too lazy to fix this mem leak...
            DefaultTexture = Texture.Create(1, 1, ColorFormat.RGBA);
            uint col = 0xffff00ff;
            DefaultTexture.GetValue().SetData(&col, (uint)sizeof(uint));

            StbImage.stbi_set_flip_vertically_on_load(1);
            
            var gapi = RendererApi.GetApi();
            Debug.Assert(gapi == GraphicsApi.OpenGL || gapi == GraphicsApi.OpenGLES);
        }

        public unsafe static WeakReference<Texture> LoadTexture(byte[] filedata)
        {
            var gapi = RendererApi.GetApi();

            ImageResult result = ImageResult.FromMemory(filedata, ColorComponents.RedGreenBlue);

            var texture = Texture.Create((uint)result.Width, (uint)result.Height, ColorFormat.RGB);
            fixed (void* p = result.Data)
                ((GLTexture)texture.GetValue()).SetData(p, (uint)result.Width, (uint)result.Height, ColorFormat.RGB, ColorFormat.RGB);

            return texture;
        }

        public unsafe static WeakReference<Texture> LoadTexture(Stream filedata)
        {
            var gapi = RendererApi.GetApi();

            ImageResult result = ImageResult.FromStream(filedata, ColorComponents.RedGreenBlue);

            var texture = Texture.Create((uint)result.Width, (uint)result.Height, ColorFormat.RGB);
            fixed (void* p = result.Data)
                ((GLTexture)texture.GetValue()).SetData(p, (uint)result.Width, (uint)result.Height, ColorFormat.RGB, ColorFormat.RGB);

            return texture;
        }
    }
}