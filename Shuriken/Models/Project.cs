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
    public static class Project
    {
        private static ObservableCollection<UIScene> scenes = new ObservableCollection<UIScene>();
        public static ObservableCollection<UIScene> Scenes
        {
            get { return scenes; }
            set { scenes = value; }
        }

        private static ObservableCollection<TextureList> texLists = new ObservableCollection<TextureList>();
        public static ObservableCollection<TextureList> TextureLists
        {
            get { return texLists; }
            set { texLists = value; }
        }

        private static ObservableCollection<UIFont> fonts = new ObservableCollection<UIFont>();
        public static ObservableCollection<UIFont> Fonts
        {
            get { return fonts; }
            set { fonts = value; }
        }


        public static void Clear()
        {
            Scenes.Clear();
            TextureLists.Clear();
            Fonts.Clear();
        }
    }
}
