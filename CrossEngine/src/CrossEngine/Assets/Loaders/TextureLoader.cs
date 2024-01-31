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
using CrossEngine.Assets.Loaders;
using System.ComponentModel.Design;
using Silk.NET.Core.Native;
using static System.Net.Mime.MediaTypeNames;

namespace CrossEngine.Assets.Loaders
{
    public class TextureLoader : GpuLoader
    {
        // TODO: shutdown

        public static WeakReference<Texture> DefaultTexture;

        // wtf, how is this not crashing the whole thing?
        // (the static ctor is very funky - nobody knows when or how it gets called
        // however we know it's before the first call to any member
        // and i can say for certain that it's even on the same thread)
        //static TextureLoader()
        //{
        //    
        //}

        public override void Init()
        {
            InternalInit();
        }

        public override void Shutdown()
        {

        }

        internal static unsafe void InternalInit()
        {
            if (DefaultTexture != null)
                return;

            // me too lazy to fix this mem leak...
            DefaultTexture = Texture.Create(1, 1, ColorFormat.RGBA);
            uint col = 0xffff00ff;
            DefaultTexture.GetValue().SetData(&col, (uint)sizeof(uint));

            StbImage.stbi_set_flip_vertically_on_load(1);

            var gapi = RendererApi.GetApi();
            Debug.Assert(gapi == GraphicsApi.OpenGL || gapi == GraphicsApi.OpenGLES);
        }

        public WeakReference<Texture> ScheduleTextureLoad(byte[] filedata)
        {
            ImageResult result = ImageResult.FromMemory(filedata, ColorComponents.RedGreenBlue);
            return ScheduledInternalLoad(result);
        }

        public WeakReference<Texture> ScheduleTextureLoad(Stream filedata)
        {
            ImageResult result = ImageResult.FromStream(filedata, ColorComponents.RedGreenBlue);
            return ScheduledInternalLoad(result);
        }

        public void ScheduleTextureUnload(WeakReference<Texture> tex)
        {
            Schedule(() => tex.GetValue().Dispose());
        }

        public static WeakReference<Texture> LoadTexture(byte[] filedata)
        {
            ImageResult result = ImageResult.FromMemory(filedata, ColorComponents.RedGreenBlue);
            return InternalLoad(result);
        }

        public static WeakReference<Texture> LoadTexture(Stream filedata)
        {
            ImageResult result = ImageResult.FromStream(filedata, ColorComponents.RedGreenBlue);
            return InternalLoad(result);
        }

        private WeakReference<Texture> ScheduledInternalLoad(ImageResult result)
        {
            WeakReference<Texture> tex = new WeakReference<Texture>(null);
            Schedule(() => InternalLoad(result, tex));
            return tex;
        }

        private static unsafe WeakReference<Texture> InternalLoad(ImageResult result, WeakReference<Texture> wr = null)
        {
            var gapi = RendererApi.GetApi();

            WeakReference<Texture> texture = (wr == null) ?
                Texture.Create((uint)result.Width, (uint)result.Height, ColorFormat.RGB) :
                Texture.Create(wr, (uint)result.Width, (uint)result.Height, ColorFormat.RGB);

            fixed (void* p = result.Data)
                ((GLTexture)texture.GetValue()).SetData(p, (uint)result.Width, (uint)result.Height, ColorFormat.RGB, ColorFormat.RGB);

            return texture;
        }
    }
}