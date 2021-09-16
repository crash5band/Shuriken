using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Shuriken.Models;

namespace Shuriken.Views
{
    /// <summary>
    /// Interaction logic for ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        public Color SelectedColor { get; private set; }

        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        private void ColorPicker_Canceled(object sender, EventArgs e)
        {
            DialogResult = false;
        }

        private void ColorPicker_Confirmed(object sender, HandyControl.Data.FunctionEventArgs<System.Windows.Media.Color> e)
        {
            SelectedColor = new Color(ColorPicker.SelectedBrush.Color);
            DialogResult = true;
        }
    }
}
