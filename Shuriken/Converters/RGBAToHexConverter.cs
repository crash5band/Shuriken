using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using Shuriken.Models;

namespace Shuriken.Converters
{
    public class RGBAToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo info)
        {
            Color rgba = value as Color;
            StringBuilder builder = new StringBuilder(9);

            builder.Append("#");
            builder.Append(rgba.A.ToString("x2"));
            builder.Append(rgba.B.ToString("x2"));
            builder.Append(rgba.G.ToString("x2"));
            builder.Append(rgba.R.ToString("x2"));

            return builder.ToString().ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo info)
        {
            string hex = value as string;
            if (hex.Length == 9)
            {
                if (hex.StartsWith("#"))
                {
                    byte r = byte.Parse(hex.Substring(1, 2), NumberStyles.HexNumber);
                    byte g = byte.Parse(hex.Substring(3, 2), NumberStyles.HexNumber);
                    byte b = byte.Parse(hex.Substring(5, 2), NumberStyles.HexNumber);
                    byte a = byte.Parse(hex.Substring(7, 2), NumberStyles.HexNumber);
                    return new Color(r, g, b, a);
                }
            }

            return new Color();
        }
    }
}
