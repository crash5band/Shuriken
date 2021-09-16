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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shuriken.Models;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for Vector2Edit.xaml
    /// </summary>
    public partial class Vector2Control : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(Vector2), typeof(Vector2Control), new PropertyMetadata(new Vector2(), new PropertyChangedCallback(OnValueChanged)));

        public event PropertyChangedEventHandler PropertyChanged;

        public Vector2 Value
        {
            get
            {
                return (Vector2)GetValue(ValueProperty);
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        public float X
        {
            get
            {
                return Value.X;
            }
            set
            {
                Value = new Vector2(value, Value.Y);
                NotifyPropertyChanged("X");
            }
        }

        public float Y
        {
            get
            {
                return Value.Y;
            }
            set
            {
                Value = new Vector2(Value.X, value);
                NotifyPropertyChanged("Y");
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Vector2Control ctrl = d as Vector2Control;
            ctrl.ChangeValue(e);
        }

        private void ChangeValue(DependencyPropertyChangedEventArgs e)
        {
            Value = (Vector2)e.NewValue;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Vector2Control()
        {
            InitializeComponent();
        }
    }
}
