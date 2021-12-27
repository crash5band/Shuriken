using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Shuriken.Rendering
{
    internal class Quad : IComparable<Quad>
    {
        public Matrix4x4 M { get; private set; }
        public Vector4 Color { get; private set; }
        public Vector4[] Gradients { get; private set; }
        public Vector2[] UVCoords { get; private set; }
        public Models.Sprite Sprite { get; private set; }

        public Vector4 TopRight => Gradients[0];
        public Vector4 BottomRight => Gradients[1];
        public Vector4 BottomLeft => Gradients[2];
        public Vector4 TopLeft => Gradients[3];

        public int Order { get; private set; }


        public Quad(Matrix4x4 m, Vector2[] uvs, Vector4 color, Vector4 tl, Vector4 tr, Vector4 bl, Vector4 br, Models.Sprite spr, int order)
        {
            M = m;
            Color = color;
            Gradients = new Vector4[4] { tr, br, bl, tl };
            UVCoords = uvs;
            Sprite = spr;
            Order = order;
        }

        public int CompareTo(Quad q)
        {
            return Order - q.Order;
        }
    }
}
