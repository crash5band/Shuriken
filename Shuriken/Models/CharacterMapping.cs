using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        private int spriteIndex;
        public int SpriteIndex
        {
            get => spriteIndex;
            set
            {
                spriteIndex = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CharacterMapping(char c, int s)
        {
            Character = c;
            SpriteIndex = s;
        }

        public CharacterMapping()
        {

        }
    }
}
