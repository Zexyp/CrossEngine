using System;
using System.Globalization;
using System.Numerics;

namespace CrossEngine.Utils.Structs;

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

    public override readonly string ToString()
    {
        return ToString(CultureInfo.CurrentCulture);
    }

    public readonly string ToString(IFormatProvider formatProvider)
    {
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

        return $"({X.ToString(formatProvider)}{separator} {Y.ToString(formatProvider)})";
    }
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

    public override readonly string ToString()
    {
        return ToString(CultureInfo.CurrentCulture);
    }

    public readonly string ToString(IFormatProvider formatProvider)
    {
        string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;

        return $"({X.ToString(formatProvider)}{separator} {Y.ToString(formatProvider)}{separator} {Z.ToString(formatProvider)})";
    }
}