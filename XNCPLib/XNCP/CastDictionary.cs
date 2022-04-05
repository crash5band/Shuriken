using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class CastDictionary
    {
        public string Name { get; set; }
        public uint NameOffset { get; set; }
        public uint GroupIndex { get; set; }
        public uint CastIndex { get; set; }

        public CastDictionary()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            NameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(NameOffset);

            GroupIndex = reader.ReadUInt32();
            CastIndex = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(NameOffset);
            writer.WriteStringOffset(NameOffset, Name);

            writer.WriteUInt32(GroupIndex);
            writer.WriteUInt32(CastIndex);
        }
    }
}
