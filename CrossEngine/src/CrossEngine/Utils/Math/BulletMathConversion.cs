#define BULLET_SINGLE_PRECISION

using System.Runtime.CompilerServices;

namespace CrossEngine.Utils.Bullet
{
    public static class BulletMathConversion
    {

#if BULLET_SINGLE_PRECISION
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BulletSharp.Math.Vector3 ToBullet(this System.Numerics.Vector3 v) => *(BulletSharp.Math.Vector3*)&v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe System.Numerics.Vector3 ToNumerics(this BulletSharp.Math.Vector3 v) => *(System.Numerics.Vector3*)&v;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BulletSharp.Math.Vector4 ToBullet(this System.Numerics.Vector4 v) => *(BulletSharp.Math.Vector4*)&v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe System.Numerics.Vector4 ToNumerics(this BulletSharp.Math.Vector4 v) => *(System.Numerics.Vector4*)&v;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe BulletSharp.Math.Matrix ToBullet(this System.Numerics.Matrix4x4 v) => *(BulletSharp.Math.Matrix*)&v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe System.Numerics.Matrix4x4 ToNumerics(this BulletSharp.Math.Matrix v) => *(System.Numerics.Matrix4x4*)&v;
#else
        public static unsafe BulletSharp.Math.Vector3 ToBullet(this System.Numerics.Vector3 v)
        {
            return new BulletSharp.Math.Vector3(v.X, v.Y, v.Z);
        }
        public static unsafe System.Numerics.Vector3 ToNumerics(this BulletSharp.Math.Vector3 v)
        {
            return new System.Numerics.Vector3((float)v.X, (float)v.Y, (float)v.Z);
        }

        public static unsafe BulletSharp.Math.Matrix ToBullet(this System.Numerics.Matrix4x4 v)
        {
            BulletSharp.Math.Matrix matrix;
            matrix.M11 = v.M11;
            matrix.M12 = v.M12;
            matrix.M13 = v.M13;
            matrix.M14 = v.M14;
            matrix.M21 = v.M21;
            matrix.M22 = v.M22;
            matrix.M23 = v.M23;
            matrix.M24 = v.M24;
            matrix.M31 = v.M31;
            matrix.M32 = v.M32;
            matrix.M33 = v.M33;
            matrix.M34 = v.M34;
            matrix.M41 = v.M41;
            matrix.M42 = v.M42;
            matrix.M43 = v.M43;
            matrix.M44 = v.M44;
            return matrix;
        }
        public static unsafe System.Numerics.Matrix4x4 ToNumerics(this BulletSharp.Math.Matrix v)
        {
            System.Numerics.Matrix4x4 matrix;
            matrix.M11 = (float)v.M11;
            matrix.M12 = (float)v.M12;
            matrix.M13 = (float)v.M13;
            matrix.M14 = (float)v.M14;
            matrix.M21 = (float)v.M21;
            matrix.M22 = (float)v.M22;
            matrix.M23 = (float)v.M23;
            matrix.M24 = (float)v.M24;
            matrix.M31 = (float)v.M31;
            matrix.M32 = (float)v.M32;
            matrix.M33 = (float)v.M33;
            matrix.M34 = (float)v.M34;
            matrix.M41 = (float)v.M41;
            matrix.M42 = (float)v.M42;
            matrix.M43 = (float)v.M43;
            matrix.M44 = (float)v.M44;
            return matrix;
        }
#endif
    }
}
