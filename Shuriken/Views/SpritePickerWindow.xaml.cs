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

namespace Shuriken.Views
{
    /// <summary>
    /// Interaction logic for SpritePickerWindow.xaml
    /// </summary>
    public partial class SpritePickerWindow : Window
    {
        public SpritePickerWindow()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;

            if (TextureLists.Count > 0)
                TextureListSelect.SelectedIndex = 0;
        }

        public Sprite SelectedSprite { get; set; }

        private void SelectClicked(object sender, EventArgs e)
        {
            SelectedSprite = SpriteList.SelectedItem as Sprite;
            DialogResult = true;
        }

        public ObservableCollection<TextureList> TextureLists => Project.TextureLists;
    }
}
