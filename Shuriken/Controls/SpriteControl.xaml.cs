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
using Shuriken.Models;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for SpriteControl.xaml
    /// </summary>
    public partial class SpriteControl : UserControl
    {
        public Sprite Sprite
        {
            get => (Sprite)GetValue(SpriteProperty);
            set => SetValue(SpriteProperty, value);
        }

        private static readonly DependencyProperty SpriteProperty = DependencyProperty.Register(
            "Sprite", typeof(Sprite), typeof(SpriteControl), new PropertyMetadata(null));

        public SpriteControl()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }
    }
}
