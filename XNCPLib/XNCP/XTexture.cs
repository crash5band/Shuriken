using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class XTexture
    {
        public StringOffset Name { get; set; }
        public uint Field04 { get; set; }

        public XTexture()
        {
            Name = new StringOffset();
        }

        public void Read(EndianBinaryReader reader)
        {
            Name.Read(reader);
            Field04 = reader.ReadUInt32();
        }
    }
}
