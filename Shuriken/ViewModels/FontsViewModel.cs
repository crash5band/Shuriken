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
        private UIFont font;
        public UIFont SelectedFont
        {
            get { return font; }
            set { font = value; NotifyPropertyChanged(); }
        }

        private CharacterMapping mapping;
        public CharacterMapping SelectedMapping
        {
            get { return mapping; }
            set { mapping = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<UIFont> Fonts => Project.Fonts;

        private RelayCommand createFontCmd;
        public RelayCommand CreateFontCommand
        {
            get { return createFontCmd ?? new RelayCommand(CreateFont, null); }
            set { createFontCmd = value; NotifyPropertyChanged();}
        }

        private RelayCommand removeFontCmd;
        public RelayCommand RemoveFontCommand
        {
            get { return removeFontCmd ?? new RelayCommand(RemoveFont, () => SelectedFont != null); }
            set { removeFontCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand createCharDefCmd;
        public RelayCommand CreateCharDefCommand
        {
            get { return createCharDefCmd ?? new RelayCommand(CreateCharacterDefinition, () => SelectedFont != null); }
            set { createCharDefCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand removeCharDefCmd;
        public RelayCommand RemoveCharDefCmd
        {
            get { return removeCharDefCmd ?? new RelayCommand(RemoveCharacterDefinition, () => SelectedMapping != null); }
            set { removeCharDefCmd = value; NotifyPropertyChanged(); }
        }

        private RelayCommand<CharacterMapping> changeMappingSpriteCmd;
        public RelayCommand<CharacterMapping> ChangeMappingSpriteCmd
        {
            get { return changeMappingSpriteCmd ?? new RelayCommand<CharacterMapping>(SelectSprite, null); }
            set { changeMappingSpriteCmd = value;}
        }

        public void CreateFont()
        {
            Fonts.Add(new UIFont("new_font"));
        }

        public void RemoveFont()
        {
            if (SelectedFont != null)
                Fonts.Remove(SelectedFont);
        }

        public void CreateCharacterDefinition()
        {
            if (SelectedFont != null)
            {
                SelectedFont.Mappings.Add(new CharacterMapping() { Character = '.', Sprite = null });
            }
        }

        public void RemoveCharacterDefinition()
        {
            if (SelectedFont != null && SelectedMapping != null)
                SelectedFont.Mappings.Remove(SelectedMapping);
        }

        public void ChangeMappingSprite(CharacterMapping mapping, Sprite spr)
        {
            mapping.Sprite = spr;
        }

        public void SelectSprite(object mapping)
        {
            SpritePickerWindow dialog = new SpritePickerWindow();
            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                CharacterMapping target = mapping as CharacterMapping;
                if (target != null)
                    ChangeMappingSprite(target, dialog.SelectedSprite);
            }
        }

        public FontsViewModel()
        {
            DisplayName = "Fonts";
            IconCode = "\xf031";
        }
    }
}
