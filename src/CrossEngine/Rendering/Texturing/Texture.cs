using System;
using static OpenGL.GL;

using System.Drawing;
using System.Drawing.Imaging;

namespace CrossEngine.Rendering.Texturing
{
	public class Texture
	{
		int type = GL_TEXTURE_2D;

        public uint id = 0;

		public int width = 0, height = 0;

		//public Texture(uint id, int width, int height)
		//{
		//	this.id = id;
		//	this.width = width;
		//	this.height = height;
		//}

		public enum ColorChannel
		{
			SingleR = GL_RED,
			SingleG = GL_GREEN,
			SingleB = GL_BLUE,
			SingleA = GL_ALPHA,

			DoubleRG = GL_RG,

			TripleRGB = GL_RGB,
			TripleBGR = GL_BGR,

			QuadrupleRGBA = GL_RGBA,
			QuadrupleBGRA = GL_BGRA

			// GL_COLOR_INDEX
			// GL_LUMINANCE
			// GL_LUMINANCE_ALPHA
		}

		// simple base texture
		public unsafe Texture(uint color)
        {
			fixed (uint* idp = &id)
				glGenTextures(1, idp);

			glBindTexture(GL_TEXTURE_2D, id);

			glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 1, 1, 0, GL_RGBA, GL_UNSIGNED_BYTE, &color);

			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

			this.type = GL_TEXTURE_2D;
			this.width = 1;
			this.height = 1;
		}

		// placeholder texture
		public unsafe Texture(int width, int height, ColorChannel channels = ColorChannel.TripleRGB)
		{
			fixed (uint* idp = &id)
				glGenTextures(1, idp);

			glBindTexture(GL_TEXTURE_2D, id);

			glTexImage2D(GL_TEXTURE_2D, 0, (int)channels, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, (void*)0);

			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);

			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
			glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

			this.type = GL_TEXTURE_2D;
			this.width = width;
			this.height = height;
		}

		public unsafe Texture(Image image)
		{
			fixed (uint* idp = &id)
				glGenTextures(1, idp);

			this.type = GL_TEXTURE_2D;

			SetData(image);
		}

		// for cube maps
		public unsafe Texture(Image[] images)
        {
			// images go like so: right, left, top, bottom, front, back
			if (images.Length < 6)
			{
				Log.Error("not enough images supplied while creating cubemap!");
				return;
			}

			fixed (uint* idp = &id)
				glGenTextures(1, idp);

			glBindTexture(GL_TEXTURE_CUBE_MAP, id);

			for (int i = 0; i < 6; i++)
			{
				int width = images[i].Width;
				int height = images[i].Height;

				BitmapData data = ((Bitmap)images[i]).LockBits(new Rectangle(0, 0, images[i].Width, images[i].Height), ImageLockMode.ReadWrite, images[i].PixelFormat);

				if (width > 0 && height > 0)
				{

					switch (data.PixelFormat)
					{
						case PixelFormat.Format24bppRgb: glTexImage2D(GL_TEXTURE_CUBE_MAP_POSITIVE_X + i, 0, GL_RGB, width, height, 0, GL_BGR, GL_UNSIGNED_BYTE, data.Scan0); break;
						default: Log.Warn("another texture format (texture will not be loaded)"); break;
					}

					glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_R, GL_CLAMP_TO_EDGE);
					glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
					glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

					glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
					glTexParameteri(GL_TEXTURE_CUBE_MAP, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
				}
				else
				{
					Log.Error("critical error occurred while loading texture");
				}

				((Bitmap)images[i]).UnlockBits(data);
			}

			this.type = GL_TEXTURE_CUBE_MAP;
		}

		/*
		unsafe ~Texture()
        {
			fixed (uint* idp = &id)
				glDeleteTextures(1, idp);
		}
		*/

		public unsafe void Dispose()
        {
			fixed (uint* idp = &id)
				glDeleteTextures(1, idp);
			id = 0;
		}

		public void SetFilterParameter(FilterParameter param, bool min = true, bool mag = true)
		{
			glBindTexture(type, id);

			if (min) glTexParameteri(type, GL_TEXTURE_MIN_FILTER, (int)param);
			if (mag) glTexParameteri(type, GL_TEXTURE_MAG_FILTER, (int)param);
		}

		public void SetWrapParameter(WrapParameter param, bool x = true, bool y = true)
		{
			glBindTexture(type, id);

			if (x) glTexParameteri(type, GL_TEXTURE_WRAP_S, (int)param);
			if (y) glTexParameteri(type, GL_TEXTURE_WRAP_T, (int)param);
		}

		public void Bind()
		{
			glBindTexture(type, id);
		}

		public void BindTo(int slot)
        {
			glActiveTexture(GL_TEXTURE0 + slot);
			glBindTexture(type, id);
		}

		public static void Unbind(int type = GL_TEXTURE_2D, int? slot = null)
        {
			if (slot != null)
				glActiveTexture(GL_TEXTURE0 + (int)slot);
			glBindTexture(type, 0);
		}

		public static void ActiveTextureSlot(int slot)
        {
			glActiveTexture(GL_TEXTURE0 + slot);
		}

		public void SetData(Image image)
        {
			int width = image.Width;
			int height = image.Height;

			BitmapData data = ((Bitmap)image).LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

			if (width > 0 && height > 0)
			{
				glBindTexture(GL_TEXTURE_2D, id);
				switch (data.PixelFormat)
				{
					case PixelFormat.Format32bppArgb: glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0); break;
					case PixelFormat.Format24bppRgb: glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_BGR, GL_UNSIGNED_BYTE, data.Scan0); break;
					case PixelFormat.Format8bppIndexed: glTexImage2D(GL_TEXTURE_2D, 0, GL_RED, width, height, 0, GL_RED, GL_UNSIGNED_BYTE, data.Scan0); break;
					default: Log.Warn("another texture format (texture will not be loaded)"); break;
				}
				glGenerateMipmap(GL_TEXTURE_2D);

				// texture wrapping parameters
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
				// texture filtering parameters
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
				glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
			}
			else
			{
				Log.Error("critical error occurred while loading texture!");
			}

			((Bitmap)image).UnlockBits(data);

			this.width = width;
			this.height = height;
		}
	}

	public enum FilterParameter
    {
		Linear = GL_LINEAR,
		Nearest = GL_NEAREST
	}

	public enum WrapParameter
    {
		Repeat = GL_REPEAT,
		MirroredRepeat = GL_MIRRORED_REPEAT,
		ClampToEdge = GL_CLAMP_TO_EDGE,
		ClampToBorder = GL_CLAMP_TO_BORDER
	}

	/*
	public enum MaterialTextureType
	{
		Diffuse = 1,
		Specular = 2,
		Normal = 3,
		Height = 4
	}

	public static string TypeEnumToString(MaterialTextureType type)
	{
		switch (type)
		{
			case MaterialTextureType.Diffuse: return "diffuse";
			case MaterialTextureType.Specular: return "specular";
			case MaterialTextureType.Normal: return "normal";
			case MaterialTextureType.Height: return "height";
			default: return "";
		}
	}
	*/
}
