using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shuriken.Models
{
    public class UIProject : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<UIScene> Scenes { get; set; }
        public ObservableCollection<TextureList> TextureLists { get; set; }
        public ObservableCollection<UIFont> Fonts { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UIProject(string projectName)
        {
            name = projectName;

            Scenes = new ObservableCollection<UIScene>();
            TextureLists = new ObservableCollection<TextureList>();
            Fonts = new ObservableCollection<UIFont>();
        }
    }
}
