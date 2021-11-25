using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using XNCPLib.XNCP;
using System.Runtime.CompilerServices;

namespace Shuriken.Models
{
    public class LayerGroup : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private uint field08;
        public uint Field08
        {
            get => field08;
            set { field08 = value; NotifyPropertyChanged(); }
        }

        private bool visible;
        public bool Visible
        {
            get => visible;
            set { visible = value; NotifyPropertyChanged(); }
        }

        [Browsable(false)]
        public ObservableCollection<UILayer> Layers { get; set; }

        public LayerGroup(UICastGroup castGroup, string name = "Group")
        {
            Name = name;
            Field08 = castGroup.Field08;
            Visible = true;
            Layers = new ObservableCollection<UILayer>();
        }

        public LayerGroup(string name = "Group")
        {
            Name = name;
            Visible = true;
            Layers = new ObservableCollection<UILayer>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
