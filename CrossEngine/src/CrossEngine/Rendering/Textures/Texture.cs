using System;
using static OpenGL.GL;

using System.Drawing;
using System.Drawing.Imaging;

using CrossEngine.Logging;
using CrossEngine.Assets.GC;
using CrossEngine.Serialization.Json;
using CrossEngine.Utils;

namespace CrossEngine.Rendering.Textures
{
    public class Texture : IDisposable
    {
        private uint _id = 0;
        private TextureTarget _type;
        private int _width;
        private int _height;

        public uint ID { get => _id; }
        public TextureTarget Type { get => _type; }
        public int Width { get => _width; }
        public int Height { get => _height; }

        public unsafe Texture()
        {
            fixed (uint* p = &_id)
                glGenTextures(1, p);
            Log.Core.Trace("generated texture (id: {0})", _id);
        }

        #region Other Constructors
        public unsafe Texture(Image image) : this()
        {
            //fixed (uint* p = &_id)
            //    glGenTextures(1, p);

            SetData(image);
        }

        public unsafe Texture(int width, int height, ColorChannelFormat format) : this()
        {
            //fixed (uint* p = &_id)
            //    glGenTextures(1, p);

            SetData(null, width, height, ColorChannelFormat.TripleBGR, format, false);
        }

        public unsafe Texture(uint color) : this()
        {
            //fixed (uint* p = &_id)
            //    glGenTextures(1, p);

            SetData(&color, 1, 1, ColorChannelFormat.QuadrupleRGBA, ColorChannelFormat.QuadrupleRGBA, false);
        }
        #endregion

        #region IDisposable
        // cleanup
        ~Texture()
        {
            Log.Core.Warn("unhandled texture disposure (id: {0})", _id);
            //System.Diagnostics.Debug.Assert(false);
            GPUGarbageCollector.MarkObject(GPUObjectType.Texture, _id);
            return;

            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (_id != 0)
            {
                Log.Core.Trace("deleting texture (id: {0})", _id);

                fixed (uint* idp = &_id)
                    glDeleteTextures(1, idp);
                _id = 0;
            }
        }
        #endregion

        /*
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
        */

        public void SetFilterParameter(FilterParameter param, bool min = true, bool mag = true)
        {
            glBindTexture((int)_type, _id);

            if (min) glTexParameteri((int)_type, GL_TEXTURE_MIN_FILTER, (int)param);
            if (mag) glTexParameteri((int)_type, GL_TEXTURE_MAG_FILTER, (int)param);
        }

        public void SetWrapParameter(WrapParameter param, bool x = true, bool y = true)
        {
            glBindTexture((int)_type, _id);

            if (x) glTexParameteri((int)_type, GL_TEXTURE_WRAP_S, (int)param);
            if (y) glTexParameteri((int)_type, GL_TEXTURE_WRAP_T, (int)param);
        }

        public void Bind(int? slot = null)
        {
            if (slot != null)
                glActiveTexture(GL_TEXTURE0 + (int)slot);
            glBindTexture((int)_type, _id);
        }

        public static void Unbind(TextureTarget type = TextureTarget.Texture2D, int? slot = null)
        {
            if (slot != null)
                glActiveTexture(GL_TEXTURE0 + (int)slot);
            glBindTexture((int)type, 0);
        }

        public static void UnbindAll(int slotRangeLast, TextureTarget type = TextureTarget.Texture2D)
        {
            for (int i = 0; i <= slotRangeLast; i++)
            {
                glActiveTexture(GL_TEXTURE0 + i);
                glBindTexture((int)type, 0);
            }
        }

        public unsafe void SetData(void* data, int width, int height, ColorChannelFormat suppliedFormat, ColorChannelFormat desiredFormat, bool generateMipmaps = true)
        {
            _type = TextureTarget.Texture2D;

            if (width <= 0 && height <= 0)
            {
                throw new Exception("invalid texture size!");
#pragma warning disable CS0162
                return;
#pragma warning restore CS0162
            }
            glBindTexture((int)_type, _id);

            // TODO: add data type
            glTexImage2D((int)_type, 0, (int)desiredFormat, width, height, 0, (int)suppliedFormat, GL_UNSIGNED_BYTE, data);

            if (generateMipmaps) glGenerateMipmap((int)_type);

            // parameters need to be set
            SetFilterParameter(FilterParameter.Linear);
            //SetWrapParameter(WrapParameter.Repeat);

            this._width = width;
            this._height = height;
        }

        public unsafe void SetData(Image image)
        {
            _type = TextureTarget.Texture2D;

            ColorChannelFormat format = 0;
            ColorChannelFormat desired = 0;

            bool converted = false;
            switch (image.PixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    {
                        format = ColorChannelFormat.QuadrupleBGRA;
                        desired = ColorChannelFormat.QuadrupleRGBA;
                    }
                    break;
                case PixelFormat.Format24bppRgb:
                    {
                        format = ColorChannelFormat.TripleBGR;
                        desired = ColorChannelFormat.TripleRGB;
                    }
                    break;
                case PixelFormat.Format8bppIndexed:
                    {
                        format = ColorChannelFormat.SingleR;
                        desired = ColorChannelFormat.SingleR;
                    }
                    break;
                default:
                    {
                        converted = true;
                        if (Image.IsAlphaPixelFormat(image.PixelFormat))
                        {
                            image = ImageUtils.CreateNewWithPixelFormat(image, PixelFormat.Format32bppArgb);

                            format = ColorChannelFormat.QuadrupleBGRA;
                            desired = ColorChannelFormat.QuadrupleRGBA;
                        }
                        else
                        {
                            image = ImageUtils.CreateNewWithPixelFormat(image, PixelFormat.Format24bppRgb);

                            format = ColorChannelFormat.TripleBGR;
                            desired = ColorChannelFormat.TripleRGB;
                        }
                        Log.Core.Trace("image format converted");

                        //Log.Core.Warn("another texture format (texture will not be loaded)");
                    }
                    break;
            }

            BitmapData data = ((Bitmap)image).LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);
            SetData(data.Scan0.ToPointer(), data.Width, data.Height, format, desired);
            ((Bitmap)image).UnlockBits(data);
            if (converted) image.Dispose();
        }
    }

    public enum FilterParameter : int
    {
        Linear = GL_LINEAR,
        Nearest = GL_NEAREST
    }

    public enum WrapParameter : int
    {
        Repeat = GL_REPEAT,
        MirroredRepeat = GL_MIRRORED_REPEAT,
        ClampToEdge = GL_CLAMP_TO_EDGE,
        ClampToBorder = GL_CLAMP_TO_BORDER
    }

    public enum TextureTarget : int
    {
        Texture1D = GL_TEXTURE_1D,
        Texture2D = GL_TEXTURE_2D,
        Texture3D = GL_TEXTURE_3D,

        TextureCubeMap = GL_TEXTURE_CUBE_MAP,

        //TextureCubeMapPositiveX = GL_TEXTURE_CUBE_MAP_POSITIVE_X,
        //TextureCubeMapNegativeX = GL_TEXTURE_CUBE_MAP_NEGATIVE_X,
        //TextureCubeMapPositiveY = GL_TEXTURE_CUBE_MAP_POSITIVE_Y,
        //TextureCubeMapNegativeY = GL_TEXTURE_CUBE_MAP_NEGATIVE_Y,
        //TextureCubeMapPositiveZ = GL_TEXTURE_CUBE_MAP_POSITIVE_Z,
        //TextureCubeMapNegativeZ = GL_TEXTURE_CUBE_MAP_NEGATIVE_Z,
    }

    public enum ColorChannelFormat : int
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
