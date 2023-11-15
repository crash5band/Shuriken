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
    public class Sprite : INotifyPropertyChanged
    {
        public readonly int ID;
        public Vector2 Start { get; set; }
        public Vector2 Dimensions { get; set; }
        public Texture Texture { get; set; }

        public int CropX { get; set; }
        public int CropY { get; set; }
        public int CropW { get; set; }
        public int CropH { get; set; }

        public int X
        {
            get { return (int)Start.X; }
            set { Start.X = value; CreateCrop(); }
        }

        public int Y
        {
            get { return (int)Start.Y; }
            set { Start.Y = value; CreateCrop(); }
        }

        public int Width
        {
            get { return (int)Dimensions.X; }
            set
            {
                Dimensions.X = value;
                if (X + Dimensions.X <= Texture.Width)
                {
                    CreateCrop();
                }
            }
        }

        public int Height
        {
            get { return (int)Dimensions.Y; }
            set
            {
                Dimensions.Y = value;
                if (Y + Dimensions.Y <= Texture.Height)
                {
                    CreateCrop();
                }
            }
        }

        public CroppedBitmap CropImg { get; set; }

        private void CreateCrop()
        {
            if (X + Width <= Texture.Width && Y + Height <= Texture.Height)
            {
                CropX = X;
                CropY = Y;
                if (Width > 0 && Height > 0)
                {
                    CropW = Width;
                    CropH = Height;
                    CropImg = new CroppedBitmap(Texture.ImageSource, new Int32Rect(CropX, CropY, CropW, CropH));
                }
            }
        }

        public Sprite(int id, Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            ID = id;
            Texture = tex;

            Start = new Vector2(MathF.Round(left * tex.Width), MathF.Round(top * tex.Height));
            Start.X = Math.Clamp(Start.X, 0, Texture.Width);
            Start.Y = Math.Clamp(Start.Y, 0, Texture.Height);

            Dimensions = new Vector2(MathF.Round((right - left) * tex.Width), MathF.Round((bottom - top) * tex.Height));
            CreateCrop();
        }

        public Sprite()
        {
            Start = new Vector2();
            Dimensions = new Vector2();

            Texture = new Texture();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
