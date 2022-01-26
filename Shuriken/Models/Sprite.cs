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
        public Vector2 Start { get; set; }
        public Vector2 Dimensions { get; set; }
        public Texture Texture { get; set; }

        public int X
        {
            get { return (int)Start.X; }
            set { Start.X = value; NotifyPropertyChanged(); CreateCrop(); }
        }

        public int Y
        {
            get { return (int)Start.Y; }
            set { Start.Y = value; NotifyPropertyChanged(); CreateCrop(); }
        }

        public int Width
        {
            get { return (int)Dimensions.X; }
            set
            {
                if (X + value <= Texture.Width)
                {
                    Width = value;
                    NotifyPropertyChanged();
                    CreateCrop();
                }
            }
        }

        public int Height
        {
            get { return (int)Dimensions.Y; }
            set
            {
                if (Y + value <= Texture.Height)
                {
                    Height = value;
                    NotifyPropertyChanged();
                    CreateCrop();
                }
            }
        }

        private CroppedBitmap crop;
        public CroppedBitmap Crop
        {
            get => crop;
            set { crop = value; NotifyPropertyChanged(); }
        }

        private void CreateCrop()
        {
            if (X + Width <= Texture.Width && Y + Height <= Texture.Height)
                Crop = new CroppedBitmap(Texture.ImageSource, new Int32Rect(X, Y, Width, Height));
        }

        public Sprite(Texture tex, float top = 0.0f, float left = 0.0f, float bottom = 1.0f, float right = 1.0f)
        {
            Texture = tex;

            Start = new Vector2(left * tex.Width, top * tex.Height);
            Start.X = Math.Clamp(Start.X, 0, Texture.Width);
            Start.Y = Math.Clamp(Start.Y, 0, Texture.Height);

            Dimensions = new Vector2((right - left) * tex.Width, (bottom - top) * tex.Height);
            CreateCrop();
        }

        public Sprite()
        {
            Start = new Vector2();
            Dimensions = new Vector2();

            Texture = new Texture();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
