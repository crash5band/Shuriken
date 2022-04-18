using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP
{
    public class EndChunk
    {
        public uint Signature { get; set; }
        public uint[] Padding { get; set; }

        public EndChunk()
        {
            Padding = new uint[] { 0, 0, 0 };
        }

        public void Read(BinaryObjectReader reader)
        {
            // Header is always little endian
            Endianness endianPrev = reader.Endianness;
            reader.Endianness = Endianness.Little;
            {
                Signature = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            Padding[0] = reader.ReadUInt32();
            Padding[1] = reader.ReadUInt32();
            Padding[2] = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            // Header is always little endian
            Endianness endianPrev = writer.Endianness;
            writer.Endianness = Endianness.Little;
            {
                writer.WriteUInt32(Signature);
            }
            writer.Endianness = endianPrev;

            writer.WriteUInt32(Padding[0]);
            writer.WriteUInt32(Padding[1]);
            writer.WriteUInt32(Padding[2]);
        }
    }
}
