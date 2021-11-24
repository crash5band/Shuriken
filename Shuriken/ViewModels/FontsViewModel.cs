using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shuriken.Models;

namespace Shuriken.ViewModels
{
    public class FontsViewModel : ViewModelBase
    {
        private UIFont font;
        public UIFont SelectedFont
        {
            get { return font; }
            set { font = value; NotifyPropertyChanged(); }
        }

        private CharacterMapping mapping;
        public CharacterMapping SelectedMapping
        {
            get { return mapping; }
            set { mapping = value; NotifyPropertyChanged(); }
        }

        public ObservableCollection<UIFont> Fonts => Project.Fonts;

        public FontsViewModel()
        {
            DisplayName = "Fonts";
            IconCode = "\xf031";
        }
    }
}
