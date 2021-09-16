using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using Shuriken.Models;

namespace Shuriken.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color color = value as Color;
            System.Windows.Media.Color brushColor = new System.Windows.Media.Color();
            brushColor.R = color.R;
            brushColor.G = color.G;
            brushColor.B = color.B;
            brushColor.A = color.A;

            return new System.Windows.Media.SolidColorBrush(brushColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Media.SolidColorBrush brush = value as System.Windows.Media.SolidColorBrush;
            return new Color(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A);
        }
    }
}
