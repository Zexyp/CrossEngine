using System;
using System.Runtime.CompilerServices;

namespace CrossEngine.Utils.Maths;

static public class MathExt
{
    public const float ToDegConstF = 180.0f / MathF.PI;
    public const double ToDegConst = 180.0 / MathF.PI;
    public const float ToRadConstF = MathF.PI / 180.0f;
    public const double ToRadConst = MathF.PI / 180.0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public float ToRadians(float degrees)
    {
        return ToRadConstF * degrees;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public float ToDegrees(float radians)
    {
        return ToDegConstF * radians;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public double ToRadians(double degrees)
    {
        return ToRadConst * degrees;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static public double ToDegrees(double radians)
    {
        return ToDegConst * radians;
    }

    public static float Lerp(float first, float second, float amount)
    {
        return first * (1 - amount) + second * amount;
    }

    public static bool Compare(float a, float b, float precision = float.Epsilon)
    {
        return Math.Abs(a - b) <= precision * Math.Max(1.0f, Math.Max(Math.Abs(a), Math.Abs(b)));
    }
}

