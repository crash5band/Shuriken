using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Shuriken.Models
{
    public class UISceneGroup
    {
        public string Name { get; set; }
        public bool Visible { get; set; }
        public ObservableCollection<UIScene> Scenes { get; set; }
        public ObservableCollection<UISceneGroup> Children { get; set; }

        public void Clear()
        {
            Scenes.Clear();
            foreach (var child in Children)
                child.Clear();
        }

        public UISceneGroup(string name)
        {
            Name = name;
            Visible = true;
            Scenes = new ObservableCollection<UIScene>();
            Children = new ObservableCollection<UISceneGroup>();
        }
    }
}
