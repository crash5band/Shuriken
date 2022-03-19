using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models
{
    internal class CastTransform
    {
        public Vector2 Position { get; set; }
        public float ZPosition { get; set; }
        public float Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public Color Color { get; set; }

        public CastTransform()
        {
            Position = new Vector2();
            ZPosition = 0.0f;
            Rotation = 0;
            Scale = new Vector3(1, 1, 1);
            Color = new Color(255, 255, 255, 255);
        }

        public CastTransform(Vector2 position, float zPos, float rotation, Vector3 scale)
        {
            Position = position;
            ZPosition = zPos;
            Rotation = rotation;
            Scale = scale;
            Color = new Color(255, 255, 255, 255);
        }

        public CastTransform(Vector2 position, float zPos, float rotation, Vector3 scale, Color color)
        {
            Position = position;
            ZPosition = zPos;
            Rotation = rotation;
            Scale = scale;
            Color = color;
        }
    }
}
