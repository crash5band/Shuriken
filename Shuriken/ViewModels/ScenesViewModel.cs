using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;

namespace Shuriken.ViewModels
{
    public class ScenesViewModel : ViewModelBase
    {
        public ObservableCollection<UIScene> Scenes => Project.Scenes;

        private UIScene scene;
        public UIScene SelectedScene
        {
            get { return scene; }
            set { scene = value; NotifyPropertyChanged(); }
        }

        public SceneViewer Viewer { get; set; }

        public ScenesViewModel()
        {
            DisplayName = "Scenes";
            IconCode = "\xf008";
            Viewer = new SceneViewer();
        }
    }
}
