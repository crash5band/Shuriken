using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Converters;
using System.Runtime.CompilerServices;

namespace Shuriken.Models
{
    public class Color : INotifyPropertyChanged
    {
        private byte r, g, b, a;

        public byte R
        {
            get
            {
                return r;
            }
            set
            {
                if (r != value)
                {
                    r = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte G
        {
            get
            {
                return g;
            }
            set
            {
                if (g != value)
                {
                    g = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte B
        {
            get
            {
                return b;
            }
            set
            {
                if (b != value)
                {
                    b = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public byte A
        {
            get
            {
                return a;
            }
            set
            {
                if (a != value)
                {
                    a = value;
                    NotifyPropertyChanged();
                }
            }
        }

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

        public OpenTK.Mathematics.Vector4 ToFloats()
        {
            return new OpenTK.Mathematics.Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return "<" + string.Join(", ", new byte[] { R, G, B, A }) + ">";
        }
    }
}
