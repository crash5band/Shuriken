using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shuriken.Converters
{
    public class ValueDescriptionMap
    {
        public object Value { get; set; }
        public object Description { get; set; }

        public ValueDescriptionMap(object v, object d)
        {
            Value = v;
            Description = d;
        }
    }
}
