using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP
{
    public class ChunkHeader
    {
        public uint Signature { get; set; }
        public uint Size { get; set; }
        public uint EndPosition { get; set; }

        public void Read(BinaryObjectReader reader)
        {
            bool bigEndian = reader.Endianness == Endianness.Big;

            // Header is always little endian
            reader.Endianness = Endianness.Little;

            long startPosition = reader.Position;
            Signature = reader.ReadUInt32();
            Size = reader.ReadUInt32();
            EndPosition = (uint)(startPosition + 8 + Size);

            if (bigEndian)
            {
                reader.Endianness = Endianness.Big;
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            bool bigEndian = writer.Endianness == Endianness.Big;

            // Header is always little endian
            writer.Endianness = Endianness.Little;

            writer.WriteUInt32(Signature);
            writer.WriteUInt32(Size);

            if (bigEndian)
            {
                writer.Endianness = Endianness.Big;
            }
        }
    }
}
