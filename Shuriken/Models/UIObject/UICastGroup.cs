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
                    name = value;
            }
        }

        public uint Field08 { get; set; }
        public bool Visible { get; set; }

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

        public UICastGroup(UICastGroup g)
        {
            Name = g.name;
            Field08 = g.Field08;
            Visible = true;

            Casts = new ObservableCollection<UICast>(g.Casts);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
