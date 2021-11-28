using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Shuriken.Models
{
    public class Sprite
    {
        public Vector2 Start { get; set; }
        public Vector2 Dimensions { get; set; }
        public Texture Texture { get; set; }

        public int X
        {
            get { return (int)Start.X; }
        }

        public int Y
        {
            get { return (int)Start.Y; }
        }

        public int Width
        {
            get { return (int)Dimensions.X; }
        }

        public int Height
        {
            get { return (int)Dimensions.Y; }
        }

        public CroppedBitmap Crop => new CroppedBitmap(Texture.ImageSource, new Int32Rect(X, Y, Width, Height));

        public Sprite(Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            Texture = tex;
            Start = new Vector2(left * tex.Width, top * tex.Height);
            Dimensions = new Vector2((right - left) * tex.Width, (bottom - top) * tex.Height);
        }

        public Sprite()
        {
            Start = new Vector2();
            Dimensions = new Vector2();

            Texture = new Texture();
        }
    }
}
