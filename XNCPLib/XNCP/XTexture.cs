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
        public uint NameOffset { get; set; }
        public uint Field04 { get; set; }

        public XTexture()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            NameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(NameOffset);
            Field04 = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(NameOffset);
            writer.WriteStringOffset(NameOffset, Name);
            writer.WriteUInt32(Field04);
        }
    }
}
