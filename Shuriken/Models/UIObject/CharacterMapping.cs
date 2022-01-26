using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Shuriken.ViewModels;

namespace Shuriken.Models
{
    public class CharacterMapping : INotifyPropertyChanged
    {
        private char character;
        public char Character
        {
            get => character;
            set
            {
                if (!string.IsNullOrEmpty(value.ToString()))
                {
                    character = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Sprite sprite;
        public Sprite Sprite
        {
            get => sprite;
            set
            {
                sprite = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CharacterMapping(char c, Sprite spr)
        {
            character = c;
            sprite = spr;
        }

        public CharacterMapping()
        {
            Sprite = new Sprite();
        }
    }
}
