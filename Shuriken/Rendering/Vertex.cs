using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Shuriken.Rendering
{
    public struct Vertex
    {
        public Vector4 Position { get; set; }
        public Vector4 Color { get; set; }
        public Vector2 UV { get; set; }

        public Vertex(Vector4 pos, Vector4 color, Vector2 uv)
        {
            Position = pos;
            Color = color;
            UV = uv;
        }
    }
}
