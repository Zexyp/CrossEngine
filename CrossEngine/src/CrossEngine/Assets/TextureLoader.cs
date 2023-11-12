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

namespace CrossEngine.Assets
{
    public static class TextureLoader
    {
        // wtf, how is this not crashing the whole thing?
        public static readonly WeakReference<Texture> DefaultTexture;

        static unsafe TextureLoader()
        {
            DefaultTexture = Texture.Create(1, 1, ColorFormat.RGBA);
            uint col = 0xffff00ff;
            DefaultTexture.GetValue().SetData(&col, (uint)sizeof(uint));

            StbImage.stbi_set_flip_vertically_on_load(1);
        }

        public unsafe static WeakReference<Texture> LoadTexture(byte[] filedata)
        {
            var gapi = RendererApi.GetApi();
            Debug.Assert(gapi == GraphicsApi.OpenGL || gapi == GraphicsApi.OpenGLES);

            ImageResult result = ImageResult.FromMemory(filedata, ColorComponents.RedGreenBlue);

            var texture = Texture.Create((uint)result.Width, (uint)result.Height, ColorFormat.RGB);
            fixed (void* p = result.Data)
                ((GLTexture)texture.GetValue()).SetData(p, (uint)result.Width, (uint)result.Height, ColorFormat.RGB, ColorFormat.RGB);

            return texture;
        }
    }
}