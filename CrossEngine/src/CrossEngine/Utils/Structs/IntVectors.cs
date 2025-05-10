using System;
using System.Globalization;
using System.Numerics;

// comma is a stupid decimal sepearator in programming

namespace CrossEngine.Utils.Structs
{
    public struct IntVec2 : IEquatable<IntVec2>, IFormattable
    {
        public int X;
        public int Y;

        public static IntVec2 Zero => new IntVec2(0);
        public static IntVec2 One => new IntVec2(1);
        public static IntVec2 UnitX => new IntVec2(1, 0);
        public static IntVec2 UnitY => new IntVec2(0, 1);

        public IntVec2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public IntVec2(int value)
        {
            this.X = value;
            this.Y = value;
        }

        public static IntVec2 operator +(IntVec2 left, IntVec2 right) => new IntVec2(left.X + right.X, left.Y + right.Y);
        public static IntVec2 operator -(IntVec2 value) => new IntVec2(-value.X, -value.Y);
        public static IntVec2 operator -(IntVec2 left, IntVec2 right) => new IntVec2(left.X - right.X, left.Y - right.Y);
        public static IntVec2 operator *(IntVec2 left, int right) => new IntVec2(left.X * right, left.Y * right);
        public static Vector2 operator *(IntVec2 left, float right) => new Vector2(left.X * right, left.Y * right);
        public static IntVec2 operator *(IntVec2 left, IntVec2 right) => new IntVec2(left.X * right.X, left.Y * right.Y);
        public static IntVec2 operator *(int left, IntVec2 right) => new IntVec2(left * right.X, left * right.Y);
        public static Vector2 operator *(float left, IntVec2 right) => new Vector2(left * right.X, left * right.Y);
        public static IntVec2 operator /(IntVec2 left, IntVec2 right) => new IntVec2(left.X / right.X, left.Y / right.Y);
        public static IntVec2 operator /(IntVec2 value1, int value2) => new IntVec2(value1.X / value2, value1.Y / value2);
        public static Vector2 operator /(IntVec2 value1, float value2) => new Vector2(value1.X / value2, value1.Y / value2);
        public static bool operator ==(IntVec2 left, IntVec2 right) => left.X == right.X && left.Y == right.Y;
        public static bool operator !=(IntVec2 left, IntVec2 right) => !(left.X == right.X && left.Y == right.Y);

        public static implicit operator Vector2(IntVec2 v) => new Vector2(v.X, v.Y);
        public static explicit operator IntVec2(Vector2 v) => new IntVec2((int)v.X, (int)v.Y);

        public override bool Equals(object obj) => obj is IntVec2 && this == (IntVec2) obj;
        public bool Equals(IntVec2 other) => this == other;

        public override readonly string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

            return $"({X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)})";
        }
    }

    public struct IntVec3 : IEquatable<IntVec3>, IFormattable
    {
        public int X;
        public int Y;
        public int Z;

        public static IntVec3 Zero => new IntVec3(0);
        public static IntVec3 One => new IntVec3(1);
        public static IntVec3 UnitX => new IntVec3(1, 0, 0);
        public static IntVec3 UnitY => new IntVec3(0, 1, 0);
        public static IntVec3 UnitZ => new IntVec3(0, 0, 1);

        public IntVec3(IntVec2 vec2, int z)
        {
            this.X = vec2.X;
            this.Y = vec2.Y;
            this.Z = z;
        }
        public IntVec3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public IntVec3(int value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        public static IntVec3 operator +(IntVec3 left, IntVec3 right) => new IntVec3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        public static IntVec3 operator -(IntVec3 value) => new IntVec3(-value.X, -value.Y, -value.Z);
        public static IntVec3 operator -(IntVec3 left, IntVec3 right) => new IntVec3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        public static IntVec3 operator *(IntVec3 left, int right) => new IntVec3(left.X * right, left.Y * right, left.Z * right);
        public static Vector3 operator *(IntVec3 left, float right) => new Vector3(left.X * right, left.Y * right, left.Z * right);
        public static IntVec3 operator *(IntVec3 left, IntVec3 right) => new IntVec3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        public static IntVec3 operator *(int left, IntVec3 right) => new IntVec3(left * right.X, left * right.Y, left * right.Z);
        public static Vector3 operator *(float left, IntVec3 right) => new Vector3(left * right.X, left * right.Y, left * right.Z);
        public static IntVec3 operator /(IntVec3 left, IntVec3 right) => new IntVec3(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
        public static IntVec3 operator /(IntVec3 value1, int value2) => new IntVec3(value1.X / value2, value1.Y / value2, value1.Z / value2);
        public static Vector3 operator /(IntVec3 value1, float value2) => new Vector3(value1.X / value2, value1.Y / value2, value1.Z / value2);
        public static bool operator ==(IntVec3 left, IntVec3 right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        public static bool operator !=(IntVec3 left, IntVec3 right) => !(left.X == right.X && left.Y == right.Y && left.Z == right.Z);

        public static implicit operator Vector3(IntVec3 v) => new Vector3(v.X, v.Y, v.Z);
        public static explicit operator IntVec3(Vector3 v) => new IntVec3((int)v.X, (int)v.Y, (int)v.Z);

        public override bool Equals(object obj) => obj is IntVec3 && this == (IntVec3)obj;
        public bool Equals(IntVec3 other) => this == other;

        public override readonly string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

            return $"({X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}{separator} {Z.ToString(format, formatProvider)})";
        }
    }

    public struct IntVec4 : IEquatable<IntVec4>, IFormattable
    {
        public int X;
        public int Y;
        public int Z;
        public int W;

        public static IntVec4 Zero => new IntVec4(0);
        public static IntVec4 One => new IntVec4(1);
        public static IntVec4 UnitX => new IntVec4(1, 0, 0, 0);
        public static IntVec4 UnitY => new IntVec4(0, 1, 0, 0);
        public static IntVec4 UnitZ => new IntVec4(0, 0, 1, 0);
        public static IntVec4 UnitW => new IntVec4(0, 0, 0, 1);

        public IntVec4(IntVec3 vec3, int w)
        {
            this.X = vec3.X;
            this.Y = vec3.Y;
            this.Z = vec3.Z;
            this.W = w;
        }
        public IntVec4(int x, int y, int z, int w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
        public IntVec4(int value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
            this.W = value;
        }

        public static IntVec4 operator +(IntVec4 left, IntVec4 right) => new IntVec4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
        public static IntVec4 operator -(IntVec4 value) => new IntVec4(-value.X, -value.Y, -value.Z, -value.W);
        public static IntVec4 operator -(IntVec4 left, IntVec4 right) => new IntVec4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
        public static IntVec4 operator *(IntVec4 left, int right) => new IntVec4(left.X * right, left.Y * right, left.Z * right, left.W * right);
        public static Vector4 operator *(IntVec4 left, float right) => new Vector4(left.X * right, left.Y * right, left.Z * right, left.W * right);
        public static IntVec4 operator *(IntVec4 left, IntVec4 right) => new IntVec4(left.X * right.X, left.Y * right.Y, left.Z * right.Z, left.W * right.W);
        public static IntVec4 operator *(int left, IntVec4 right) => new IntVec4(left * right.X, left * right.Y, left * right.Z, left * right.W);
        public static Vector4 operator *(float left, IntVec4 right) => new Vector4(left * right.X, left * right.Y, left * right.Z, left * right.W);
        public static IntVec4 operator /(IntVec4 left, IntVec4 right) => new IntVec4(left.X / right.X, left.Y / right.Y, left.Z / right.Z, left.W / right.W);
        public static IntVec4 operator /(IntVec4 value1, int value2) => new IntVec4(value1.X / value2, value1.Y / value2, value1.Z / value2, value1.W / value2);
        public static Vector4 operator /(IntVec4 value1, float value2) => new Vector4(value1.X / value2, value1.Y / value2, value1.Z / value2, value1.W / value2);
        public static bool operator ==(IntVec4 left, IntVec4 right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W;
        public static bool operator !=(IntVec4 left, IntVec4 right) => !(left.X == right.X && left.Y == right.Y && left.Z == right.Z && left.W == right.W);

        public static implicit operator Vector4(IntVec4 v) => new Vector4(v.X, v.Y, v.Z, v.W);
        public static explicit operator IntVec4(Vector4 v) => new IntVec4((int)v.X, (int)v.Y, (int)v.Z, (int)v.W);

        public override bool Equals(object obj) => obj is IntVec4 && this == (IntVec4)obj;
        public bool Equals(IntVec4 other) => this == other;

        public override readonly string ToString()
        {
            return ToString("G", CultureInfo.CurrentCulture);
        }

        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

            return $"({X.ToString(format, formatProvider)}{separator} {Y.ToString(format, formatProvider)}{separator} {Z.ToString(format, formatProvider)} {W.ToString(format, formatProvider)})";
        }
    }
}
