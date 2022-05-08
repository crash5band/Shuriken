using System;
using System.Numerics;

namespace Shuriken.Misc.Extensions;

public static class NumericsEx
{
    public static Vector2 Rotate(this Vector2 value, float angle)
    {
        return new Vector2(
            value.X * MathF.Cos(angle) + value.Y * MathF.Sin(angle), 
            value.Y * MathF.Cos(angle) - value.X * MathF.Sin(angle));
    }
}