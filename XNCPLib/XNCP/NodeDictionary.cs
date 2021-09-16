using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XNCPLib.XNCP
{
    public class NodeDictionary
    {
        public StringOffset Name { get; set; }
        public uint Index { get; set; }

        public NodeDictionary()
        {
            Name = new StringOffset();
        }
    }
}
