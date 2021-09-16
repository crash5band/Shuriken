using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Shuriken.Models;
using System.Windows.Media.Imaging;

namespace Shuriken.ViewModels
{
    class TextureViewModel : ViewModelBase
    {
        private Texture texture;
        
        public string Name
        {
            get
            {
                return texture.Name;
            }
        }

        public int Width
        {
            get
            {
                return texture.Width;
            }
        }

        public int Height
        {
            get
            {
                return texture.Height;
            }
        }

        public BitmapSource Image
        {
            get
            {
                return texture.ImageSource;
            }
            set
            {
                texture.ImageSource = value;
                NotifyPropertyChanged();
            }
        }

        public TextureViewModel(Texture t)
        {
            texture = t;
        }

        public TextureViewModel()
        {

        }
    }
}
