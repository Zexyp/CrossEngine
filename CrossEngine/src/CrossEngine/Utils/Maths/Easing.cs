using System;

namespace CrossEngine.Utils.Maths;

// this was translated using machine translation, expect the unexpected!
[Obsolete]
public static class Easing
{
    public static double InSine(double x) => 1 - Math.Cos((x * Math.PI) / 2);
    public static double OutSine(double x) => Math.Sin((x * Math.PI) / 2);
    public static double InOutSine(double x) => -(Math.Cos(Math.PI * x) - 1) / 2;

    public static double InQuad(double x) => x * x;
    public static double OutQuad(double x) => 1 - (1 - x) * (1 - x);
    public static double InOutQuad(double x) => x < 0.5 ? 2 * x * x : 1 - Math.Pow(-2 * x + 2, 2) / 2;

    public static double InCubic(double x) => x * x * x;
    public static double OutCubic(double x) => 1 - Math.Pow(1 - x, 3);
    public static double InOutCubic(double x) => x < 0.5 ? 4 * x * x * x : 1 - Math.Pow(-2 * x + 2, 3) / 2;

    public static double InQuart(double x) => x * x * x * x;
    public static double OutQuart(double x) => 1 - Math.Pow(1 - x, 4);
    public static double InOutQuart(double x) => x < 0.5 ? 8 * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 4) / 2;

    public static double InQuint(double x) => x * x * x * x * x;
    public static double OutQuint(double x) => 1 - Math.Pow(1 - x, 5);
    public static double InOutQuint(double x) => x < 0.5 ? 16 * x * x * x * x * x : 1 - Math.Pow(-2 * x + 2, 5) / 2;

    public static double InExpo(double x) => x == 0 ? 0 : Math.Pow(2, 10 * x - 10);
    public static double OutExpo(double x) => x == 1 ? 1 : 1 - Math.Pow(2, -10 * x);
    public static double InOutExpo(double x) => x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? Math.Pow(2, 20 * x - 10) / 2 : (2 - Math.Pow(2, -20 * x + 10)) / 2;

    public static double InCirc(double x) => 1 - Math.Sqrt(1 - Math.Pow(x, 2));
    public static double OutCirc(double x) => Math.Sqrt(1 - Math.Pow(x - 1, 2));
    public static double InOutCirc(double x) => x < 0.5 ? (1 - Math.Sqrt(1 - Math.Pow(2 * x, 2))) / 2 : (Math.Sqrt(1 - Math.Pow(-2 * x + 2, 2)) + 1) / 2;

    public static double InBack(double x)
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return c3 * x * x * x - c1 * x * x;
    }

    public static double OutBack(double x)
    {
        const double c1 = 1.70158;
        const double c3 = c1 + 1;
        return 1 + c3 * Math.Pow(x - 1, 3) + c1 * Math.Pow(x - 1, 2);
    }

    public static double InOutBack(double x)
    {
        const double c1 = 1.70158;
        const double c2 = c1 * 1.525;
        return x < 0.5 ? (Math.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2 : (Math.Pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }

    public static double InElastic(double x)
    {
        const double c4 = (2 * Math.PI) / 3;
        return x == 0 ? 0 : x == 1 ? 1 : -Math.Pow(2, 10 * x - 10) * Math.Sin((x * 10 - 10.75) * c4);
    }

    public static double OutElastic(double x)
    {
        const double c4 = (2 * Math.PI) / 3;
        return x == 0 ? 0 : x == 1 ? 1 : Math.Pow(2, -10 * x) * Math.Sin((x * 10 - 0.75) * c4) + 1;
    }

    public static double InOutElastic(double x)
    {
        const double c5 = (2 * Math.PI) / 4.5;
        return x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? -(Math.Pow(2, 20 * x - 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 : (Math.Pow(2, -20 * x + 10) * Math.Sin((20 * x - 11.125) * c5)) / 2 + 1;
    }

    public static double InBounce(double x) => 1 - OutBounce(1 - x);
    
    public static double OutBounce(double x)
    {
        const double n1 = 7.5625;
        const double d1 = 2.75;
        if (x < 1 / d1) return n1 * x * x;
        if (x < 2 / d1) return n1 * (x -= 1.5 / d1) * x + 0.75;
        if (x < 2.5 / d1) return n1 * (x -= 2.25 / d1) * x + 0.9375;
        return n1 * (x -= 2.625 / d1) * x + 0.984375;
    }

    public static double InOutBounce(double x) => x < 0.5 ? (1 - OutBounce(1 - 2 * x)) / 2 : (1 + OutBounce(2 * x - 1)) / 2;
}

[Obsolete]
public static class EasingF
{
    public static float InSine(float x) => 1 - MathF.Cos((x * MathF.PI) / 2);
    public static float OutSine(float x) => MathF.Sin((x * MathF.PI) / 2);
    public static float InOutSine(float x) => -(MathF.Cos(MathF.PI * x) - 1) / 2;

    public static float InQuad(float x) => x * x;
    public static float OutQuad(float x) => 1 - (1 - x) * (1 - x);
    public static float InOutQuad(float x) => x < 0.5f ? 2 * x * x : 1 - MathF.Pow(-2 * x + 2, 2) / 2;

    public static float InCubic(float x) => x * x * x;
    public static float OutCubic(float x) => 1 - MathF.Pow(1 - x, 3);
    public static float InOutCubic(float x) => x < 0.5f ? 4 * x * x * x : 1 - MathF.Pow(-2 * x + 2, 3) / 2;

    public static float InQuart(float x) => x * x * x * x;
    public static float OutQuart(float x) => 1 - MathF.Pow(1 - x, 4);
    public static float InOutQuart(float x) => x < 0.5f ? 8 * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 4) / 2;

    public static float InQuint(float x) => x * x * x * x * x;
    public static float OutQuint(float x) => 1 - MathF.Pow(1 - x, 5);
    public static float InOutQuint(float x) => x < 0.5f ? 16 * x * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 5) / 2;

    public static float InExpo(float x) => x == 0 ? 0 : MathF.Pow(2, 10 * x - 10);
    public static float OutExpo(float x) => x == 1 ? 1 : 1 - MathF.Pow(2, -10 * x);
    public static float InOutExpo(float x) => x == 0 ? 0 : x == 1 ? 1 : x < 0.5f ? MathF.Pow(2, 20 * x - 10) / 2 : (2 - MathF.Pow(2, -20 * x + 10)) / 2;

    public static float InCirc(float x) => 1 - MathF.Sqrt(1 - x * x);
    public static float OutCirc(float x) => MathF.Sqrt(1 - (x - 1) * (x - 1));
    public static float InOutCirc(float x) => x < 0.5f ? (1 - MathF.Sqrt(1 - 4 * x * x)) / 2 : (MathF.Sqrt(1 - (-2 * x + 2) * (-2 * x + 2)) + 1) / 2;

    public static float InBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return c3 * x * x * x - c1 * x * x;
    }

    public static float OutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
    }

    public static float InOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;
        return x < 0.5f ? MathF.Pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2) / 2 : (MathF.Pow(2 * x - 2, 2) * ((c2 + 1) * (2 * x - 2) + c2) + 2) / 2;
    }

    public static float InBounce(float x) => 1 - OutBounce(1 - x);
    
    public static float OutBounce(float x)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        if (x < 1 / d1) return n1 * x * x;
        if (x < 2 / d1) return n1 * (x -= 1.5f / d1) * x + 0.75f;
        if (x < 2.5f / d1) return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        return n1 * (x -= 2.625f / d1) * x + 0.984375f;
    }

    public static float InOutBounce(float x) => x < 0.5f ? (1 - OutBounce(1 - 2 * x)) / 2 : (1 + OutBounce(2 * x - 1)) / 2;
}
