using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;
using Shuriken.Commands;
using Microsoft.Win32;

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
            set { removeTextureCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand createTexListCmd;
        public RelayCommand CreateTexListCmd
        {
            get { return createTexListCmd ?? new RelayCommand(CreateTexList, null); }
            set { createTexListCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand removeTexListCmd;
        public RelayCommand RemoveTexListCmd
        {
            get { return removeTexListCmd ?? new RelayCommand(RemoveTexList, () => SelectedTexList != null); }
            set { removeTexListCmd = value; NotifyPropertyChanged(); }
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
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Direct Draw Surface Textures |*.dds";

                if (dlg.ShowDialog() == true)
                {
                    SelectedTexList.Textures.Add(new Texture(dlg.FileName));
                }
            }
        }

        public void RemoveTexture()
        {
            if (SelectedTexList != null && SelectedTexture != null)
                SelectedTexList.Textures.Remove(SelectedTexture);
        }

        public void CreateTexList()
        {
            TextureLists.Add(new TextureList("tex_list"));
        }

        public void RemoveTexList()
        {
            if (SelectedTexList != null)
                TextureLists.Remove(SelectedTexList);
        }

        public SpritesViewModel()
        {
            DisplayName = "Sprites";
            IconCode = "\xf302";
        }
    }
}
