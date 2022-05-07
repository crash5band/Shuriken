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
        public Matrix4x4 M;
        public Vector4 Color;
        public Vector2 UV0;
        public Vector2 UV1;
        public Vector2 UV2;
        public Vector2 UV3;
        public Models.Sprite Sprite;
        public Vector4 TopRight;
        public Vector4 BottomRight;
        public Vector4 BottomLeft;
        public Vector4 TopLeft;
        public int Order;
        public bool Additive;
        
        public Quad(Matrix4x4 m, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector4 color, 
            Vector4 tl, Vector4 tr, Vector4 bl, Vector4 br, Models.Sprite spr, int order, bool additive)
        {
            M = m;
            UV0 = uv0;
            UV1 = uv1;
            UV2 = uv2;
            UV3 = uv3;
            Color = color;
            TopRight = tr;
            BottomRight = br;
            BottomLeft = bl;
            TopLeft = tl;
            Sprite = spr;
            Order = order;
            Additive = additive;
        }

        public int CompareTo(Quad q)
        {
            return Order - q.Order;
        }
    }
}
