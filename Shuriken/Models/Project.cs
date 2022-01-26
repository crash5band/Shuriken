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
    public class Project
    {
        public ObservableCollection<UIScene> Scenes { get; set; }
        public ObservableCollection<TextureList> TextureLists { get; set; }
        public ObservableCollection<UIFont> Fonts { get; set; }

        public void Clear()
        {
            Scenes.Clear();
            TextureLists.Clear();
            Fonts.Clear();
        }

        public Project()
        {
            Scenes = new ObservableCollection<UIScene>();
            TextureLists = new ObservableCollection<TextureList>();
            Fonts = new ObservableCollection<UIFont>();
        }
    }
}
