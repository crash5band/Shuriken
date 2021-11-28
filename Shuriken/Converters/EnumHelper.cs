using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Converters
{
    public class EnumHelper
    {
        public static string Description(Enum value)
        {
            var attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Any())
            {
                return (attributes.First() as DescriptionAttribute).Description;
            }

            return value.ToString();
        }

        public static IEnumerable<ValueDescriptionMap> GetAllValuesAndDescriptions(Type t)
        {
            if (!t.IsEnum)
            {
                throw new ArgumentException($"{nameof(t)} must be an enum type.");
            }

            return Enum.GetValues(t).Cast<Enum>().Select((e) => new ValueDescriptionMap(e, Description(e))).ToList();
        }
    }
}
