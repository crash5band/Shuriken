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
    public partial class Vector2Control : UserControl
    {
        /// <summary>
        /// Gets or sets the value of the bound Vector2 object
        /// </summary>
        public Vector2 Value
        {
            get => (Vector2)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(Vector2), typeof(Vector2Control), new PropertyMetadata(new Vector2()));

        public Vector2Control()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
    }
}
