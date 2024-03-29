﻿using System;

using System.Numerics;

namespace CrossEngine.Utils
{
    public struct IntVec2 : IEquatable<IntVec2>, IFormattable
    {
        public int X;
        public int Y;

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
        public static IntVec2 operator *(IntVec2 left, float right) => new IntVec2((int)(left.X * right), (int)(left.Y * right));
        public static IntVec2 operator *(IntVec2 left, IntVec2 right) => new IntVec2(left.X * right.X, left.Y * right.Y);
        public static IntVec2 operator *(int left, IntVec2 right) => new IntVec2(left * right.X, left * right.Y);
        public static IntVec2 operator *(float left, IntVec2 right) => new IntVec2((int)(left * right.X), (int)(left * right.Y));
        public static IntVec2 operator /(IntVec2 left, IntVec2 right) => new IntVec2(left.X / right.X, left.Y / right.Y);
        public static IntVec2 operator /(IntVec2 value1, int value2) => new IntVec2(value1.X / value2, value1.Y / value2);
        public static IntVec2 operator /(IntVec2 value1, float value2) => new IntVec2((int)(value1.X / value2), (int)(value1.Y / value2));
        public static bool operator ==(IntVec2 left, IntVec2 right) => left.X == right.X && left.Y == right.Y;
        public static bool operator !=(IntVec2 left, IntVec2 right) => !(left.X == right.X && left.Y == right.Y);

        public static implicit operator Vector2(IntVec2 v) => new Vector2(v.X, v.Y);
        public static explicit operator IntVec2(Vector2 v) => new IntVec2((int)v.X, (int)v.Y);

        public override bool Equals(object obj) => obj is IntVec2 && this == (IntVec2) obj;
        public bool Equals(IntVec2 other) => this == other;

        public string ToString(string format, IFormatProvider formatProvider) => $"({X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)})";
    }

    public struct IntVec3 : IEquatable<IntVec3>, IFormattable
    {
        public int X;
        public int Y;
        public int Z;

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
        public static IntVec3 operator *(IntVec3 left, float right) => new IntVec3((int)(left.X * right), (int)(left.Y * right), (int)(left.Z * right));
        public static IntVec3 operator *(IntVec3 left, IntVec3 right) => new IntVec3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        public static IntVec3 operator *(int left, IntVec3 right) => new IntVec3(left * right.X, left * right.Y, left * right.Z);
        public static IntVec3 operator *(float left, IntVec3 right) => new IntVec3((int)(left * right.X), (int)(left * right.Y), (int)(left * right.Z));
        public static IntVec3 operator /(IntVec3 left, IntVec3 right) => new IntVec3(left.X / right.X, left.Y / right.Y, left.Y / right.Y);
        public static IntVec3 operator /(IntVec3 value1, int value2) => new IntVec3(value1.X / value2, value1.Y / value2, value1.Z / value2);
        public static IntVec3 operator /(IntVec3 value1, float value2) => new IntVec3((int)(value1.X / value2), (int)(value1.Y / value2), (int)(value1.Z / value2));
        public static bool operator ==(IntVec3 left, IntVec3 right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        public static bool operator !=(IntVec3 left, IntVec3 right) => !(left.X == right.X && left.Y == right.Y && left.Z == right.Z);

        public static implicit operator Vector3(IntVec3 v) => new Vector3(v.X, v.Y, v.Z);
        public static explicit operator IntVec3(Vector3 v) => new IntVec3((int)v.X, (int)v.Y, (int)v.Z);

        public override bool Equals(object obj) => obj is IntVec3 && this == (IntVec3)obj;
        public bool Equals(IntVec3 other) => this == other;

        public string ToString(string format, IFormatProvider formatProvider) => $"({X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)}, {Z.ToString(format, formatProvider)})";
    }

    public struct BoolVec2 : IEquatable<BoolVec2>
    {
        public bool X;
        public bool Y;

        public BoolVec2(bool x, bool y)
        {
            this.X = x;
            this.Y = y;
        }
        public BoolVec2(bool value)
        {
            this.X = value;
            this.Y = value;
        }

        public static BoolVec2 operator !(BoolVec2 v) => new BoolVec2(!v.X, !v.Y);
        public static BoolVec2 operator &(BoolVec2 left, BoolVec2 right) => new BoolVec2(left.X && right.X, left.Y && right.Y);
        public static BoolVec2 operator |(BoolVec2 left, BoolVec2 right) => new BoolVec2(left.X || right.X, left.Y || right.Y);
        public static bool operator ==(BoolVec2 left, BoolVec2 right) => left.X == right.X && left.Y == right.Y;
        public static bool operator !=(BoolVec2 left, BoolVec2 right) => !(left.X == right.X && left.Y == right.Y);

        public static implicit operator IntVec2(BoolVec2 v) => new IntVec2(v.X ? 1 : 0, v.Y ? 1 : 0);
        public static explicit operator BoolVec2(IntVec2 v) => new BoolVec2(v.X != 0, v.Y != 0);
        public static explicit operator BoolVec2(Vector2 v) => new BoolVec2(v.X != 0, v.Y != 0);

        public override bool Equals(object obj) => obj is BoolVec2 && this == (BoolVec2)obj;
        public bool Equals(BoolVec2 other) => this == other;

        public string ToString(IFormatProvider formatProvider) => $"({X.ToString(formatProvider)}, {Y.ToString(formatProvider)})";
    }

    public struct BoolVec3 : IEquatable<BoolVec3>
    {
        public bool X;
        public bool Y;
        public bool Z;

        public BoolVec3(bool x, bool y, bool z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        public BoolVec3(bool value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        public static BoolVec3 operator !(BoolVec3 v) => new BoolVec3(!v.X, !v.Y, !v.Z);
        public static BoolVec3 operator &(BoolVec3 left, BoolVec3 right) => new BoolVec3(left.X && right.X, left.Y && right.Y, left.Z && right.Z);
        public static BoolVec3 operator |(BoolVec3 left, BoolVec3 right) => new BoolVec3(left.X || right.X, left.Y || right.Y, left.Z || right.Z);
        public static bool operator ==(BoolVec3 left, BoolVec3 right) => left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        public static bool operator !=(BoolVec3 left, BoolVec3 right) => !(left.X == right.X && left.Y == right.Y && left.Z == right.Z);

        public static implicit operator IntVec3(BoolVec3 v) => new IntVec3(v.X ? 1 : 0, v.Y ? 1 : 0, v.Z ? 1 : 0);
        public static explicit operator BoolVec3(IntVec3 v) => new BoolVec3(v.X != 0, v.Y != 0, v.Z != 0);
        public static explicit operator BoolVec3(Vector3 v) => new BoolVec3(v.X != 0, v.Y != 0, v.Z != 0);

        public override bool Equals(object obj) => obj is BoolVec3 && this == (BoolVec3)obj;
        public bool Equals(BoolVec3 other) => this == other;

        public string ToString(IFormatProvider formatProvider) => $"({X.ToString(formatProvider)}, {Y.ToString(formatProvider)}, {Z.ToString(formatProvider)})";
    }
}
