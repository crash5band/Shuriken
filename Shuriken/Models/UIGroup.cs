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
    public class UIGroup : INotifyPropertyChanged
    {
        private bool visible;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Category("Group")]
        public string Name { get; set; }

        [Category("Group")]
        public uint Field08 { get; set; }

        [Browsable(false)]
        public ObservableCollection<UILayer> Layers { get; set; }

        [Browsable(false)]
        public bool Visible
        {
            get => visible;
            set
            {
                visible = value;
                NotifyPropertyChanged();
            }
        }

        public UIGroup(UICastGroup castGroup, string name = "UIGroup")
        {
            Name = name;
            Field08 = castGroup.Field08;
            Visible = false;
            Layers = new ObservableCollection<UILayer>();
        }

        public UIGroup(string name = "UIGroup")
        {
            Name = name;
            Visible = false;
            Layers = new ObservableCollection<UILayer>();
        }
    }
}
