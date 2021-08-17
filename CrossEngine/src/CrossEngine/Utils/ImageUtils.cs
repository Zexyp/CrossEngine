using System;
using static OpenGL.GL;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections.Generic;
using System.Numerics;



namespace CrossEngine.Utils
{
	public static class ImageUtils
    {
		public static Image CreateNewWithPixelFormat(Image image, PixelFormat desiredPixelFormat)
        {
			return ((Bitmap)image).Clone(new Rectangle(0, 0, image.Width, image.Height), desiredPixelFormat);
		}

		public enum ColorChannel
        {
			Red = 0,
			Green = 1,
			Blue = 2,
			Alpha = 3
        }

		public static void SwapChannels(Image image, ColorChannel colorChannel1, ColorChannel colorChannel2)
        {
			if (colorChannel1 == colorChannel2)
				throw new InvalidEnumArgumentException("Cannot swap the same channel!");

			if (!(image.PixelFormat == PixelFormat.Format24bppRgb || image.PixelFormat == PixelFormat.Format32bppArgb))
				return;

			if((int)colorChannel1 > (int)colorChannel2)
            {
				ColorChannel hColorChannel = colorChannel1;
				colorChannel1 = colorChannel2;
				colorChannel2 = hColorChannel;
			}

			Bitmap bmp = (Bitmap)image;
			BitmapData bmpData = ((Bitmap)image).LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

			IntPtr ptr = bmpData.Scan0;

			int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
			byte[] rgbValues = new byte[bytes];

			System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

			int offset = 0;
			switch(image.PixelFormat)
            {
				case PixelFormat.Format32bppArgb: offset = 4; break;
				case PixelFormat.Format24bppRgb: offset = 3; break;
				default: throw new InvalidDataException("The images has unsupported format!");
			}

			for (int counter = (int)colorChannel1; counter < rgbValues.Length; counter += offset)
            {
				byte helper = rgbValues[counter];
				rgbValues[counter] = rgbValues[counter + (int)colorChannel2];
				rgbValues[counter + (int)colorChannel2] = helper;
			}

			System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

			bmp.UnlockBits(bmpData);
		}
	}
}
