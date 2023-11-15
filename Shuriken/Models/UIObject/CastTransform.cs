namespace Shuriken.Models
{
    public class CastTransform
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public Color Color { get; set; }

        public CastTransform()
        {
            Position = new Vector2();
            Rotation = 0;
            Scale = new Vector2(1, 1);
            Color = new Color(255, 255, 255, 255);
        }

        public CastTransform(Vector2 position, float rotation, Vector2 scale, Color color)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Color = color;
        }
    }
}
