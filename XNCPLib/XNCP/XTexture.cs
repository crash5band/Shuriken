using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class XTexture
    {
        public string Name { get; set; }
        public uint Field04 { get; set; }

        public XTexture()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            uint nameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(nameOffset);
            Field04 = reader.ReadUInt32();
        }
    }
}
