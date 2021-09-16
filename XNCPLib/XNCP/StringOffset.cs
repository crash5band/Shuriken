using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class StringOffset
    {
        public uint Offset { get; set; }
        public string Value { get; set; }

        public void Read(EndianBinaryReader reader)
        {
            Offset = reader.ReadUInt32();
            long pos = reader.Position;

            if (Offset != 0)
            {
                reader.SeekBegin(reader.PeekBaseOffset());
                reader.SeekCurrent(Offset);
                Value = reader.ReadString(StringBinaryFormat.NullTerminated);
                reader.SeekBegin(pos);
            }
        }
    }
}
