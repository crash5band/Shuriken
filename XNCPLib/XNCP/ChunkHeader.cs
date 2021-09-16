using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class ChunkHeader
    {
        public uint Signature { get; set; }
        public uint Size { get; set; }
        public uint EndPosition { get; set; }

        public void Read(EndianBinaryReader reader)
        {
            bool bigEndian = reader.Endianness == Endianness.BigEndian;

            // Header is always little endian
            reader.Endianness = Endianness.LittleEndian;

            long startPosition = reader.Position;
            Signature = reader.ReadUInt32();
            Size = reader.ReadUInt32();
            EndPosition = (uint)(startPosition + 8 + Size);

            if (bigEndian)
            {
                reader.Endianness = Endianness.BigEndian;
            }
        }
    }
}
