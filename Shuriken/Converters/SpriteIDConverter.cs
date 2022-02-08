using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;  
using Shuriken.Models;

namespace Shuriken.Converters
{
    public class SpriteIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Sprite spr)
                return spr.ID;
            else if (value is int)
                return Project.TryGetSprite((int)value);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Sprite spr)
                return spr.ID;
            else if (value is int)
                return Project.TryGetSprite((int)value);

            return -1;
        }
    }
}
