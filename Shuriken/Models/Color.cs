using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Shuriken.Models
{
    public class Color : INotifyPropertyChanged
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public Color(byte r = 0, byte g = 0, byte b = 0, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(uint color)
        {
            R = (byte)(color >> 24);
            G = (byte)(color >> 16);
            B = (byte)(color >> 8);
            A = (byte)(color >> 0);
        }

        public Color(System.Drawing.Color c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
            A = c.A;
        }

        public Color(System.Windows.Media.Color c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
            A = c.A;
        }

        public Color(float r, float g, float b, float a)
        {
            R = (byte)(r * 255);
            G = (byte)(g * 255);
            B = (byte)(b * 255);
            A = (byte)(a * 255);
        }

        public Color(float col) : this((uint)BitConverter.SingleToInt32Bits(col))
        {
        }

        public Color(Color c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
            A = c.A;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Vector4 ToFloats()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        public uint ToUint()
        {
            return (uint)(R << 24 | G << 16 | B << 8 | A);
        }

        public float ToFloat()
        {
            return BitConverter.Int32BitsToSingle((int)ToUint());
        }

        public override string ToString()
        {
            return $"<{R}, {G}, {B}, {A}>";
        }
    }
}
