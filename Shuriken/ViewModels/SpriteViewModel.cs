using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using System.Windows;
using Microsoft.Win32;
using Shuriken.Models;
using Shuriken.Commands;

namespace Shuriken.ViewModels
{
    public class SpriteViewModel : ViewModelBase
    {
        private Sprite sprite;
        private CroppedBitmap image;
        private RelayCommand changeTextureCmd;

        [Browsable(false)]
        public Sprite Sprite
        {
            get
            {
                return sprite;
            }
            set
            {
                sprite = value;
                NotifyPropertyChanged();
            }
        }

        [Category("Position")]
        public int X
        {
            get
            {
                return (int)sprite.Start.X;
            }
            set
            {
                if (value + Width <= sprite.Texture.Width && value >= 0)
                {
                    sprite.Start.X = value;
                    NotifyPropertyChanged();
                    CreateSprite();
                }
            }
        }

        [Category("Position")]
        public int Y
        {
            get
            {
                return (int)sprite.Start.Y;
            }
            set
            {
                if (value + Height <= sprite.Texture.Height && value >= 0)
                {
                    sprite.Start.Y = value;
                    NotifyPropertyChanged();
                    CreateSprite();
                }
            }
        }

        [Category("Dimensions")]
        public int Width
        {
            get
            {
                return (int)sprite.Dimensions.X;
            }
            set
            {
                if (value + X <= sprite.Texture.Width)
                {
                    sprite.Dimensions.X = value;
                    NotifyPropertyChanged();
                    CreateSprite();
                }
            }
        }

        [Category("Dimensions")]
        public int Height
        {
            get
            {
                return (int)sprite.Dimensions.Y;
            }
            set
            {
                if (value + Y <= sprite.Texture.Height)
                {
                    sprite.Dimensions.Y = value;
                    NotifyPropertyChanged();
                    CreateSprite();
                }
            }
        }

        [Browsable(false)]
        public Texture Texture
        {
            get
            {
                return sprite.Texture;
            }
            set
            {
                sprite.Texture = value;
                NotifyPropertyChanged();
            }
        }

        [Browsable(false)]
        public CroppedBitmap Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                NotifyPropertyChanged();
            }
        }
        
        [Browsable(false)]
        public RelayCommand ChangeTextureCmd
        {
            get => changeTextureCmd ?? new RelayCommand(ChangeTexture);
            private set { changeTextureCmd = value; }
        }

        private void CreateSprite()
        {
            if (Width > 0 && Height > 0 && X >= 0 && Y >= 0)
            {
                if (X + Width <= sprite.Texture.Width && Y + Height <= sprite.Texture.Height)
                {
                    Int32Rect crop = new Int32Rect((int)sprite.Start.X, (int)sprite.Start.Y, (int)sprite.Dimensions.X, (int)sprite.Dimensions.Y);
                    Image = new CroppedBitmap(sprite.Texture.ImageSource, crop);
                }
            }
        }

        public void ChangeTexture()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Direct Draw Surface Textures (.dds)|*.dds";

            if (dialog.ShowDialog() == true)
            {
                Texture = new Texture(dialog.FileName);

                // Reset sprite coords to avoid out of range values
                X = 0;
                Y = 0;
                Width = Texture.Width;
                Height = Texture.Height;

                CreateSprite();
            }
        }

        public SpriteViewModel(Sprite s)
        {
            sprite = s;
            CreateSprite();
        }

        public SpriteViewModel()
        {

        }
    }
}
