using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;

namespace Shuriken.ViewModels
{
    public class SpritesViewModel : ViewModelBase
    {
        private TextureList texList;
        public TextureList SelectedTexList
        {
            get { return texList; }
            set { texList = value; NotifyPropertyChanged(); }
        }

        private Texture texture;
        public Texture SelectedTexture
        {
            get { return texture; }
            set { texture = value; NotifyPropertyChanged(); }
        }

        private Sprite sprite;
        public Sprite SelectedSprite
        {
            get { return sprite; }
            set { sprite = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<TextureList> TextureLists => Project.TextureLists;

        public SpritesViewModel()
        {
            DisplayName = "Sprites";
            IconCode = "\xf302";
        }
    }
}
