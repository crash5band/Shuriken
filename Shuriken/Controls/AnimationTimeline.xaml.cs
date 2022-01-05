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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shuriken.Models.Animation;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for AnimationTimeline.xaml
    /// </summary>
    public partial class AnimationTimeline : UserControl
    {
        public ObservableCollection<AnimationGroup> Animations
        {
            get => (ObservableCollection<AnimationGroup>)GetValue(AnimationProperty);
            set => SetValue(AnimationProperty, value);
        }

        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register(
            "Animations", typeof(ObservableCollection<AnimationGroup>), typeof(AnimationTimeline), new PropertyMetadata(null));
        public AnimationTimeline()
        {
            InitializeComponent();
            Animations = new ObservableCollection<AnimationGroup>();
            LayoutRoot.DataContext = this;
        }
    }
}
