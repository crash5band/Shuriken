using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP.Animation
{
    public class AnimationDictionary
    {
        public StringOffset Name { get; set; }
        public uint Index { get; set; }

        public AnimationDictionary()
        {
            Name = new StringOffset();
        }

        public void Read(EndianBinaryReader reader)
        {
            Name.Read(reader);
            Index = reader.ReadUInt32();
        }
    }
}
