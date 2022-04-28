using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;
using Shuriken.Commands;
using Shuriken.Views;

namespace Shuriken.ViewModels
{
    public class FontsViewModel : ViewModelBase
    {
        public UIFont SelectedFont { get; set; }
        public CharacterMapping SelectedMapping { get; set; }
        public RelayCommand CreateFontCommand { get; }
        public RelayCommand RemoveFontCommand { get; }
        public RelayCommand CreateCharDefCommand { get; }
        public RelayCommand RemoveCharDefCmd { get; }
        public RelayCommand<CharacterMapping> ChangeMappingSpriteCmd { get; }

        public void CreateFont()
        {
            Project.CreateFont("new_font");
        }

        public void RemoveFont()
        {
            if (SelectedFont != null)
                Project.RemoveFont(SelectedFont.ID);
        }

        public void CreateCharacterDefinition()
        {
            if (SelectedFont != null)
            {
                SelectedFont.Mappings.Add(new CharacterMapping() { Character = '.', Sprite = -1 });
            }
        }

        public void RemoveCharacterDefinition()
        {
            if (SelectedFont != null && SelectedMapping != null)
                SelectedFont.Mappings.Remove(SelectedMapping);
        }

        public void ChangeMappingSprite(CharacterMapping mapping, int sprID)
        {
            mapping.Sprite = sprID;
        }

        public void SelectSprite(object mapping)
        {
            SpritePickerWindow dialog = new SpritePickerWindow(Project.TextureLists);
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                CharacterMapping target = mapping as CharacterMapping;
                if (target != null)
                {
                    ChangeMappingSprite(target, dialog.SelectedSpriteID);
                }
            }
        }

        public FontsViewModel()
        {
            DisplayName = "Fonts";
            IconCode = "\xf031";

            CreateFontCommand       = new RelayCommand(CreateFont, null);
            RemoveFontCommand       = new RelayCommand(RemoveFont, () => SelectedFont != null);
            CreateCharDefCommand    = new RelayCommand(CreateCharacterDefinition, () => SelectedFont != null);
            RemoveCharDefCmd        = new RelayCommand(RemoveCharacterDefinition, () => SelectedMapping != null);
            ChangeMappingSpriteCmd  = new RelayCommand<CharacterMapping>(SelectSprite, null);
        }
    }
}
