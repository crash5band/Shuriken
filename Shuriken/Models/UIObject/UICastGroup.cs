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
    public class UICastGroup : INotifyPropertyChanged, ICastContainer
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

        public ObservableCollection<UICast> Casts { get; set; }

        public void AddCast(UICast cast)
        {
            Casts.Add(cast);
        }

        public void RemoveCast(UICast cast)
        {
            Casts.Remove(cast);
        }

        public UICastGroup(CastGroup castGroup, string name = "Group")
        {
            Name = name;
            Field08 = castGroup.Field08;
            Visible = true;
            Casts = new ObservableCollection<UICast>();
        }

        public UICastGroup(string name = "Group")
        {
            Name = name;
            Visible = true;
            Casts = new ObservableCollection<UICast>();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
