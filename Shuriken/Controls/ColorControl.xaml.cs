using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shuriken.Models;
using Shuriken.Views;
using HandyControl.Controls;
using System.Runtime.CompilerServices;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(Color), typeof(ColorControl), new PropertyMetadata(new Color()));

        public Color Value
        {
            get => (Color)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private void ColorBtnClick(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow window = new ColorPickerWindow();
            System.Windows.Media.Color c = new System.Windows.Media.Color();
            c.R = Value.R;
            c.G = Value.G;
            c.B = Value.B;
            c.A = Value.A;
            window.ColorPicker.SelectedBrush = new System.Windows.Media.SolidColorBrush(c);

            window.ShowDialog();
            if (window.DialogResult == true)
            {
                Value = window.SelectedColor;
            }
        }

        public ColorControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
    }
}
