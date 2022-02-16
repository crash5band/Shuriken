using Shuriken.Models;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for Vector3Control.xaml
    /// </summary>
    public partial class Vector3Control : UserControl
    {
        public Vector3Control()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        /// <summary>
        /// Gets or sets the value of the bound Vector2 object
        /// </summary>
        public Vector3 Value
        {
            get => (Vector3)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(Vector3), typeof(Vector3Control), new PropertyMetadata(new Vector3()));

    }
}
