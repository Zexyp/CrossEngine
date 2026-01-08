using CrossEngine.Rendering.Buffers;
using CrossEngine.Rendering.Textures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CrossEngine.Platform.Windows
{
    static class GdiUtils
    {
        public static PixelFormat ToGdiPixelFormat(ColorFormat format)
        {
            switch (format)
            {
                case ColorFormat.RGBA: return PixelFormat.Format32bppArgb;
                case ColorFormat.RGB: return PixelFormat.Format24bppRgb;
            }

            Debug.Assert(false, $"Unknown {nameof(ColorFormat)} value");
            return 0;
        }
    }

    static class GdiHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte* StreamStart(UnmanagedMemoryStream stream) => stream.PositionPointer - stream.Position;
    }
}
