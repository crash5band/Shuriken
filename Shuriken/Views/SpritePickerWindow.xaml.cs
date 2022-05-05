using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Shuriken.Models;
using System.ComponentModel;

namespace Shuriken.Views
{
    /// <summary>
    /// Interaction logic for SpritePickerWindow.xaml
    /// </summary>
    public partial class SpritePickerWindow : Window, INotifyPropertyChanged
    {
        public SpritePickerWindow(IEnumerable<TextureList> texCollection)
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            TextureLists = new ObservableCollection<TextureList>(texCollection);

            if (TextureLists.Count > 0)
                TextureListSelect.SelectedIndex = 0;

            SelectedSpriteID = -1;
        }

        public bool SelectionValid { get; set; }

        private void SelectClicked(object sender, EventArgs e)
        {
            DialogResult = true;
        }
        public int SelectedTexture { get; private set; }
        public int SelectedSpriteID { get; private set; }
        public ObservableCollection<TextureList> TextureLists { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void SpriteListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedSpriteID = SpriteList.SelectedItem == null ? -1 : (int)SpriteList.SelectedItem;
            SelectionValid = SelectedSpriteID != -1;
        }

        private void TexturesListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //SelectedSpriteID = -1;
            //SelectionValid = false;
        }
    }
}
