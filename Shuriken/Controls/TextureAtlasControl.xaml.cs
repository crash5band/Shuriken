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
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shuriken.Controls
{
    /// <summary>
    /// Interaction logic for TextureAtlasControl.xaml
    /// </summary>
    public partial class TextureAtlasControl : UserControl, INotifyPropertyChanged
    {
        public TextureAtlasControl()
        {
            InitializeComponent();
            zoom = 1.0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool mouseDown = false;

        // the point at which the mouse was held down
        private Point mouseDownPos;

        private Point mousePos;
        public Point MousePos
        {
            get => mousePos;
            set
            {
                mousePos = value;
                ;
            }
        }

        private double zoom;
        public double Zoom
        {
            get => zoom;
            set
            {
                zoom = value;
                SelectionBorder.LayoutTransform = new ScaleTransform(zoom, zoom);
                ;
            }
        }

        private void GridMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture and track mouse
            mouseDown = true;
            mouseDownPos = e.GetPosition(TexImage);
            SelectionBorder.CaptureMouse();

            // Initialize selection rectangle placement
            Canvas.SetLeft(SelectionRectangle, mouseDownPos.X);
            Canvas.SetTop(SelectionRectangle, mouseDownPos.Y);
            SelectionRectangle.Width = 0;
            SelectionRectangle.Height = 0;
        }

        private void GridMouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            SelectionBorder.ReleaseMouseCapture();
        }

        private void GridMouseMove(object sender, MouseEventArgs e)
        {
            MousePos = e.GetPosition(TexImage);

            if (mouseDown)
            {
                mousePos.X = Math.Clamp(mousePos.X, 0, TexImage.Width);
                mousePos.Y = Math.Clamp(mousePos.Y, 0, TexImage.Height);

                Canvas.SetLeft(SelectionRectangle, mousePos.X > mouseDownPos.X ? mouseDownPos.X : mousePos.X);
                SelectionRectangle.Width = Math.Abs(mouseDownPos.X - mousePos.X);

                Canvas.SetTop(SelectionRectangle, mousePos.Y > mouseDownPos.Y ? mouseDownPos.Y : mousePos.Y);
                SelectionRectangle.Height = Math.Abs(mouseDownPos.Y - mousePos.Y);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
