using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shuriken.Models
{
    public class UIFont : INotifyPropertyChanged
    {
        public int ID { get; private set; }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    name = value;
            }
        }

        public ObservableCollection<CharacterMapping> Mappings { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public UIFont(string name, int id)
        {
            ID = id;
            Name = name;
            Mappings = new ObservableCollection<CharacterMapping>();
        }
    }
}
