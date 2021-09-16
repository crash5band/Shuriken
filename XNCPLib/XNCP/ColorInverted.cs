using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace XNCPLib.XNCP
{
    public struct ColorInverted
    {
        public byte A { get; set; }
        public byte B { get; set; }
        public byte G { get; set; }
        public byte R { get; set; }

        public ColorInverted(byte a, byte b, byte g, byte r)
        {
            A = a;
            B = b;
            G = g;
            R = r;
        }

        public override string ToString()
        {
            return "<" + string.Join(", ", new byte[] { A, B, G, R }) + ">";
        }

        public Color ToColor()
        {
            return Color.FromArgb(A, R, G, B);
        }
    }
}
