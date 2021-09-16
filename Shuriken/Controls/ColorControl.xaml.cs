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
    public partial class ColorControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(Color), typeof(ColorControl), new PropertyMetadata(new Color(), new PropertyChangedCallback(OnValueChanged)));

        public event PropertyChangedEventHandler PropertyChanged;

        public Color Value
        {
            get
            {
                return (Color)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
                NotifyPropertyChanged("R");
                NotifyPropertyChanged("G");
                NotifyPropertyChanged("B");
                NotifyPropertyChanged("A");
            }
        }

        public byte R
        {
            get
            {
                return Value.R;
            }
            set
            {
                Value = new Color(value, Value.G, Value.B, Value.A);
                NotifyPropertyChanged();
            }
        }

        public byte G
        {
            get
            {
                return Value.G;
            }
            set
            {
                Value = new Color(Value.R, value, Value.B, Value.A);
                NotifyPropertyChanged();
            }
        }

        public byte B
        {
            get
            {
                return Value.B;
            }
            set
            {
                Value = new Color(Value.R, Value.G, value, Value.A);
                NotifyPropertyChanged();
            }
        }

        public byte A
        {
            get
            {
                return Value.A;
            }
            set
            {
                Value = new Color(Value.R, Value.G, Value.B, value);
                NotifyPropertyChanged();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorControl ctrl = d as ColorControl;
            ctrl.ChangeValue(e);
        }

        private void ChangeValue(DependencyPropertyChangedEventArgs e)
        {
            Value = (Color)e.NewValue;
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ColorControl()
        {
            InitializeComponent();
        }

        private void ColorBtnClick(object sender, RoutedEventArgs e)
        {
            ColorPickerWindow window = new ColorPickerWindow();
            System.Windows.Media.Color c = new System.Windows.Media.Color();
            c.R = R;
            c.G = G;
            c.B = B;
            c.A = A;
            window.ColorPicker.SelectedBrush = new System.Windows.Media.SolidColorBrush(c);

            window.ShowDialog();
            if (window.DialogResult == true)
            {
                Value = window.SelectedColor;
            }
        }
    }
}
