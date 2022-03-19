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
        public uint Index { get; set; }

        public AnimationDictionary()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            uint nameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(nameOffset);
            Index = reader.ReadUInt32();
        }
    }
}
