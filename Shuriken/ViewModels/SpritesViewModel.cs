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
        public TextureList SelectedTexList { get; set; }
        public Texture SelectedTexture { get; set; }
        public Sprite SelectedSprite { get; set; }

        public RelayCommand CreateSpriteCmd { get; }
        public RelayCommand RemoveSpriteCmd { get; }
        public RelayCommand CreateTextureCmd { get; }
        public RelayCommand RemoveTextureCmd { get; }
        public RelayCommand CreateTexListCmd { get; }
        public RelayCommand RemoveTexListCmd { get; }

        public ObservableCollection<TextureList> TextureLists => Project.TextureLists;

        public void CreateSprite()
        {
            if (SelectedTexture != null)
            {
                int id = Project.CreateSprite(SelectedTexture);
                SelectedTexture.Sprites.Add(id);
            }
        }

        public void RemoveSprite()
        {
            if (SelectedTexture != null && SelectedSprite != null)
            {
                if (SelectedTexture.Sprites.Remove(SelectedSprite.ID))
                {
                    Project.RemoveSprite(SelectedSprite.ID);
                    SelectedSprite = null;
                }
            }
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
            
            CreateSpriteCmd     = new RelayCommand(CreateSprite, () => SelectedTexture != null);
            RemoveSpriteCmd     = new RelayCommand(RemoveSprite, () => SelectedSprite != null);
            CreateTextureCmd    = new RelayCommand(CreateTexture, () => SelectedTexList != null);
            RemoveTextureCmd    = new RelayCommand(RemoveTexture, () => SelectedTexture != null);
            CreateTexListCmd    = new RelayCommand(CreateTexList, null);
            RemoveTexListCmd    = new RelayCommand(RemoveTexList, () => SelectedTexList != null);
        }
    }
}
