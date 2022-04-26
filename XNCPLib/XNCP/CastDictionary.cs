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
        public uint GroupIndex { get; set; }
        public uint CastIndex { get; set; }

        public CastDictionary()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            uint nameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(nameOffset);

            GroupIndex = reader.ReadUInt32();
            CastIndex = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer, uint nameOffset)
        {
            writer.WriteUInt32(nameOffset);
            writer.WriteStringOffset(nameOffset, Name);

            writer.WriteUInt32(GroupIndex);
            writer.WriteUInt32(CastIndex);
        }
    }
}
