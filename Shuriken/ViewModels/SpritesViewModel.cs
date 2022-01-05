using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;
using Shuriken.Commands;

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

        private RelayCommand createSpriteCmd;
        public RelayCommand CreateSpriteCmd
        {
            get { return createSpriteCmd ?? new RelayCommand(CreateSprite, () => SelectedTexture != null); }
            set { createSpriteCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand removeSpriteCmd;
        public RelayCommand RemoveSpriteCmd
        {
            get { return removeSpriteCmd ?? new RelayCommand(RemoveSprite, () => SelectedSprite != null); }
            set { removeSpriteCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand createTextureCmd;
        public RelayCommand CreateTextureCmd
        {
            get { return createTextureCmd ?? new RelayCommand(CreateTexture, () => SelectedTexList != null); }
            set { createTextureCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand removeTextureCmd;
        public RelayCommand RemoveTextureCmd
        {
            get { return removeTextureCmd ?? new RelayCommand(RemoveTexture, () => SelectedTexture != null); }
            set { RemoveTextureCmd = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<TextureList> TextureLists => Project.TextureLists;

        public void CreateSprite()
        {
            if (SelectedTexture != null)
                SelectedTexture.Sprites.Add(new Sprite(SelectedTexture));
        }

        public void RemoveSprite()
        {
            if (SelectedTexture != null && SelectedSprite != null)
                SelectedTexture.Sprites.Remove(SelectedSprite);
        }

        public void CreateTexture()
        {
            if (SelectedTexList != null)
                SelectedTexList.Textures.Add(new Texture());
        }

        public void RemoveTexture()
        {
            if (SelectedTexList != null && SelectedTexture != null)
                SelectedTexList.Textures.Remove(SelectedTexture);
        }

        public SpritesViewModel()
        {
            DisplayName = "Sprites";
            IconCode = "\xf302";
        }
    }
}
