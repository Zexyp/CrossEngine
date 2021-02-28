using System;

using System.Drawing;
using System.Numerics;

namespace CrossEngine.Utils
{
    static public class MathExtension
    {
        static public double ToRadians(double degreesAngle)
        {
            return (Math.PI / 180) * degreesAngle;
        }
        static public float ToRadians(float degreesAngle)
        {
            return (MathF.PI / 180) * degreesAngle;
        }

        public static float Lerp(float first, float second, float amount)
        {
            return first * (1 - amount) + second * amount;
        }

        public static bool Compare(float a, float b, float precision = float.MinValue)
        {
            return Math.Abs(a - b) <= precision * Math.Max(1.0f, Math.Max(Math.Abs(a), Math.Abs(b)));
        }
    }

    static class Vector2Extension
    {
        public static Vector2 Rotate(Vector2 vec, float angleRad)
        {
            float cos = MathF.Cos(angleRad);
            float sin = MathF.Sin(angleRad);

            float xPrime = vec.X * cos - vec.Y * sin;
            float yPrime = vec.X * sin - vec.Y * cos;

            vec.X = xPrime;
            vec.Y = yPrime;

            return vec;
        }
        public static Vector2 RotateAroundOrigin(Vector2 vec, float angleRad, Vector2 origin)
        {
            float x = vec.X - origin.X;
            float y = vec.Y - origin.Y;

            float cos = MathF.Cos(angleRad);
            float sin = MathF.Sin(angleRad);

            vec.X = (x * cos + y * sin) + origin.X;
            vec.Y = (x * sin - y * cos) + origin.Y;

            return vec;
        }

        public static bool Compare(Vector2 a, Vector2 b, float precision = float.MinValue)
        {
            return MathExtension.Compare(a.X, b.X, precision) && MathExtension.Compare(a.Y, b.Y, precision);
        }

        public static Vector2 FromSizeF(SizeF size)
        {
            return new Vector2(size.Width, size.Height);
        }

        //public static Vector3 XYZ(this Vector2 v, float z)
        //{
        //    return new Vector3(v.X, v.Y, z);
        //}
    }

    public static class Vector3Extension
    {
        static Random random = new Random();

        public static Vector3 FromSize(Size size, float z = 0)
        {
            return new Vector3(size.Width, size.Height, z);
        }

        public static Vector3 Random()
        {
            return Vector3.Normalize(new Vector3((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f));
        }

        public static Vector2 XY(this Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static Vector3 ColorFromHex(uint color)
        {
            return new Vector3((float)(byte)(color >> 16) / 255, (float)(byte)(color >> 8) / 255, (float)(byte)color / 255);
        }

        public static Vector3 HSVToRGB(Vector3 hsvColor)
        {
            //while (hsvColor.X < 0) { hsvColor.X += 360; };
            //while (hsvColor.X >= 360) { hsvColor.X -= 360; };
            if (hsvColor.X < 0)
                hsvColor.X += 1;
            hsvColor.X = (hsvColor.X % 1.0f) * 6;
            Vector3 rgbColor = new Vector3();
            if (hsvColor.Z <= 0)
            { 
                rgbColor.X = rgbColor.Y = rgbColor.Z = 0;
            }
            else if (hsvColor.Y <= 0)
            {
                rgbColor.X = rgbColor.Y = rgbColor.Z = hsvColor.Z;
            }
            else
            {
                float hf = hsvColor.X;
                int i = (int)Math.Floor(hf);
                float f = hf - i;
                float pv = hsvColor.Z * (1 - hsvColor.Y);
                float qv = hsvColor.Z * (1 - hsvColor.Y * f);
                float tv = hsvColor.Z * (1 - hsvColor.Y * (1 - f));
                switch (i)
                {
                    case 0:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = tv;
                        rgbColor.Z = pv;
                        break;
                    case 1:
                        rgbColor.X = qv;
                        rgbColor.Y = hsvColor.Z;
                        rgbColor.Z = pv;
                        break;
                    case 2:
                        rgbColor.X = pv;
                        rgbColor.Y = hsvColor.Z;
                        rgbColor.Z = tv;
                        break;
                    case 3:
                        rgbColor.X = pv;
                        rgbColor.Y = qv;
                        rgbColor.Z = hsvColor.Z;
                        break;
                    case 4:
                        rgbColor.X = tv;
                        rgbColor.Y = pv;
                        rgbColor.Z = hsvColor.Z;
                        break;
                    case 5:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = pv;
                        rgbColor.Z = qv;
                        break;
                    case 6:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = tv;
                        rgbColor.Z = pv;
                        break;
                    case -1:
                        rgbColor.X = hsvColor.Z;
                        rgbColor.Y = pv;
                        rgbColor.Z = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        Log.Warn("color conversion messed up!");
                        rgbColor.X = rgbColor.Y = rgbColor.Z = hsvColor.Z;
                        break;
                }
            }
            return rgbColor;
        }

        public static Vector3 RGBToHSV(Vector3 rgbColor)
        {
            Vector3 hsvColor = new Vector3();
            float min, max, delta;

            min = rgbColor.X < rgbColor.Y ? rgbColor.X : rgbColor.Y;
            min = min  < rgbColor.Z ? min  : rgbColor.Z;

            max = rgbColor.X > rgbColor.Y ? rgbColor.X : rgbColor.Y;
            max = max  > rgbColor.Z ? max  : rgbColor.Z;

            hsvColor.Z = max;
            delta = max - min;

            if (delta < 0.00001f)
            {
                hsvColor.Y = 0.0f;
                hsvColor.X = 0.0f;
                return hsvColor;
            }

            if( max > 0.0f ) 
            {
                hsvColor.Y = (delta / max);
            } 
            else
            {
                hsvColor.Y = 0.0f;
                hsvColor.X = 0.0f;
                return hsvColor;
            }

            if (rgbColor.X >= max)
                hsvColor.X = (rgbColor.Y - rgbColor.Z) / delta;
            else if (rgbColor.Y >= max)
                hsvColor.X = 2.0f + (rgbColor.Z - rgbColor.X) / delta;
            else
                hsvColor.X = 4.0f + (rgbColor.X - rgbColor.Y) / delta;

            hsvColor.X /= 6.0f;

            if (hsvColor.X < 0.0f)
                hsvColor.X += 1.0f;

            return hsvColor;
        }
    }

    static class Vector4Extension
    {
        public static Vector3 XYZ(this Vector4 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector4 FromColor(System.Drawing.Color color)
        {
            return new Vector4((float)color.R / byte.MaxValue, (float)color.G / byte.MaxValue, (float)color.B / byte.MaxValue, (float)color.A / byte.MaxValue);
        }
    }

    public static class Matrix4x4Extension
    {
        public static Matrix4x4 CreateShearX(float value)
        {
            Matrix4x4 shear = Matrix4x4.Identity;
            shear.M21 = value;
            return shear;
        }
        public static Matrix4x4 CreateShearY(float value)
        {
            Matrix4x4 shear = Matrix4x4.Identity;
            shear.M32 = value;
            return shear;
        }
        public static Matrix4x4 CreateShearZ(float value)
        {
            Matrix4x4 shear = Matrix4x4.Identity;
            shear.M23 = value;
            return shear;
        }

        public static Matrix4x4 MirrorX
        {
            get
            {
                Matrix4x4 mirror = Matrix4x4.Identity;
                mirror.M11 = -1;
                return mirror;
            }
        }
        public static Matrix4x4 MirrorY
        {
            get
            {
                Matrix4x4 mirror = Matrix4x4.Identity;
                mirror.M22 = -1;
                return mirror;
            }
        }
        public static Matrix4x4 MirrorZ
        {
            get
            {
                Matrix4x4 mirror = Matrix4x4.Identity;
                mirror.M33 = -1;
                return mirror;
            }
        }

        public static Matrix4x4 MirrorAll
        {
            get
            {
                return MirrorZ * MirrorY * MirrorX;
            }
        }

        public static Matrix4x4 ClearTranslation(Matrix4x4 matrix)
        {
            matrix.M14 = 0;
            matrix.M24 = 0;
            matrix.M34 = 0;
            matrix.M41 = 0;
            matrix.M42 = 0;
            matrix.M43 = 0;
            return matrix;
        }

        public static Matrix4x4 CreateBillboard(Vector3 right, Vector3 up, Vector3 look, Vector3 pos)
        {
            Matrix4x4 matrix = new Matrix4x4();

            //   2 and 1 can be swapped to change the noraml
            //      |
            //      |
            matrix.M21 = right.X;
            matrix.M22 = right.Y;
            matrix.M23 = right.Z;
            matrix.M24 = 0;

            matrix.M11 = up.X;
            matrix.M12 = up.Y;
            matrix.M13 = up.Z;
            matrix.M14 = 0;

            matrix.M31 = look.X;
            matrix.M32 = look.Y;
            matrix.M33 = look.Z;
            matrix.M34 = 0;

            matrix.M41 = pos.X;
            matrix.M42 = pos.Y;
            matrix.M43 = pos.Z;
            matrix.M44 = 1;

            return matrix;
        }

        //static public Matrix4x4 CreatePrespective(float fovRad, float aspectRatio, float near, float far)
        //{
        //    Matrix4x4 projMat = new Matrix4x4();
        //    projMat.M11 = aspectRatio * fovRad;
        //    projMat.M22 = fovRad;
        //    projMat.M33 = far / (far - near);
        //    projMat.M43 = (-far * near) / (far - near);
        //    projMat.M34 = 1.0f;
        //    projMat.M44 = 0.0f;
        //    return projMat;
        //}
    }

    static class QuaternionExtension
    {
        public static Vector3 ToEuler(this Quaternion q)
        {
            // black box content provided by:
            // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles

            Vector3 angles = new Vector3(0.0f);

            // roll (x-axis rotation)
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                angles.Y = (float)Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public static Quaternion RotateX(float angle)
        {
            return Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), angle);
        }
        public static Quaternion RotateY(float angle)
        {
            return Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), angle);
        }
        public static Quaternion RotateZ(float angle)
        {
            return Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), angle);
        }
    }

    static class ColorExtension
    {
        public static System.Drawing.Color FromVector4(Vector4 vector)
        {
            return System.Drawing.Color.FromArgb((int)(vector.W * byte.MaxValue), (int)(vector.X * byte.MaxValue), (int)(vector.Y * byte.MaxValue), (int)(vector.Z * byte.MaxValue));
        }
    }

    static class SizeExtension
    {
        public static Size Multiply(Size a, Size b)
        {
            return new Size(a.Width * b.Width, a.Height * b.Height);
        }
    }
}
