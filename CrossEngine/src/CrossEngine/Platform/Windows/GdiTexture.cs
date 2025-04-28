using CrossEngine.Debugging;
using CrossEngine.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CrossEngine.Platform.Windows.GdiContext;

namespace CrossEngine.Platform.Windows
{
    class GdiTexture : Texture
    {
        public override uint RendererId => throw new NotImplementedException();

        public override uint Width => width;

        public override uint Height => height;

        internal Bitmap bitmap;
        uint width, height;

        public GdiTexture(uint width, uint height, ColorFormat internalFormat)
        {
            GC.KeepAlive(this);
            GPUGC.Register(this);

            this.width = width;
            this.height = height;
            bitmap = new Bitmap((int)width, (int)height, GdiUtils.ToGdiPixelFormat(internalFormat));
        }

        protected override void Dispose(bool disposing)
        {
            bitmap.Dispose();
        }

        public override void Bind(uint slot = 0)
        {
            state.samplers[slot] = this;
        }

        public override unsafe void SetData(void* data, uint size)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var pixelStride = bitmap.PixelFormat switch
            {
                PixelFormat.Format24bppRgb => 3,
                PixelFormat.Format32bppArgb => 4,
                _ => 0
            };
            Debug.Assert(pixelStride * width * height == size);

            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            
            int byteLength = Math.Abs(bmpData.Stride) * bitmap.Height;
            Buffer.MemoryCopy(data, bmpData.Scan0.ToPointer(), byteLength, byteLength);

            bitmap.UnlockBits(bmpData);
        }

        public override void SetFilterParameter(FilterParameter filter)
        {
            GdiRendererApi.Log.Warn("texture filtering not implemented");
        }

        public override void SetWrapParameter(WrapParameter wrap)
        {
            GdiRendererApi.Log.Warn("texture wrapping not implemented");
        }

        public override void Unbind()
        {
            foreach (var key in state.samplers.Keys)
            {
                if (state.samplers[key] == this)
                    state.samplers[key] = null;
            }
        }
    }
}
