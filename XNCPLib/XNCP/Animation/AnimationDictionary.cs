using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP.Animation
{
    public class AnimationDictionary
    {
        public string Name { get; set; }
        public uint NameOffset { get; set; }
        public uint Index { get; set; }

        public AnimationDictionary()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            NameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(NameOffset);
            Index = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(NameOffset);
            writer.WriteStringOffset(NameOffset, Name);
            writer.WriteUInt32(Index);
        }
    }
}
