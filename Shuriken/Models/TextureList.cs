using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Models
{
    public class TextureList : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    name = value;
            }
        }

        public ObservableCollection<Texture> Textures { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public TextureList(string listName)
        {
            name = listName;
            Textures = new ObservableCollection<Texture>();
        }
    }
}
