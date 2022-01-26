using System;
using System.ComponentModel;
using Shuriken.Converters;

namespace Shuriken.Models
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x = 0.0f, float y = 0.0f)
        {
            X = x;
            Y = y;
        }

        public Vector2(System.Numerics.Vector2 v)
        {
            X = v.X;
            Y = v.Y;
        }

        public System.Numerics.Vector2 ToSystemNumerics()
        {
            return new System.Numerics.Vector2(X, Y);
        }

        public static Vector2 operator+(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector2 operator-(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector2 operator*(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.X * v2.X, v1.Y * v2.Y);
        }

        public static implicit operator System.Numerics.Vector2(Vector2 v)
        {
            return new System.Numerics.Vector2(v.X, v.Y);
        }

        public override string ToString()
        {
            return $"<{X}, {Y}>";
        }
    }
}
