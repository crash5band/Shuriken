using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class CastDictionary
    {
        public StringOffset Name { get; set; }
        public uint GroupIndex { get; set; }
        public uint CastIndex { get; set; }

        public CastDictionary()
        {
            Name = new StringOffset();
        }

        public void Read(EndianBinaryReader reader)
        {
            Name.Read(reader);
            GroupIndex = reader.ReadUInt32();
            CastIndex = reader.ReadUInt32();
        }
    }
}
