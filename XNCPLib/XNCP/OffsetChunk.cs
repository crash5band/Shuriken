//#define VERIFY_OFFSET_TABLE

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class OffsetChunk
    {
        public uint Signature { get; set; }
        public uint Field0C { get; set; }
        public List<uint> OffsetLocations { get; set; }
#if VERIFY_OFFSET_TABLE
        public List<uint> OffsetLocationsTemp { get; set; }
#endif

        public OffsetChunk()
        {
            OffsetLocations = new List<uint>();
#if VERIFY_OFFSET_TABLE
            OffsetLocationsTemp = new List<uint>();
#endif
        }

        public void Read(BinaryObjectReader reader)
        {
            reader.PushOffsetOrigin();
            Endianness endianPrev = reader.Endianness;

            // Header is always little endian
            uint size;
            reader.Endianness = Endianness.Little;
            {
                Signature = reader.ReadUInt32();
                size = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            uint offsetLocationCount = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();

            for (int loc = 0; loc < offsetLocationCount; ++loc)
            {
#if VERIFY_OFFSET_TABLE
                OffsetLocationsTemp.Add(reader.ReadUInt32());
#endif
            }

            reader.Seek(reader.GetOffsetOrigin() + size + 8, SeekOrigin.Begin);
            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.PushOffsetOrigin();
            Endianness endianPrev = writer.Endianness;

            // Header is always little endian
            writer.Endianness = Endianness.Little;
            {
                writer.WriteUInt32(Signature);

                // Skipped: size
                writer.Skip(4);
            }
            writer.Endianness = endianPrev;
            uint dataStart = (uint)writer.Position;

            writer.WriteUInt32((uint)OffsetLocations.Count);
            writer.WriteUInt32(Field0C);

#if VERIFY_OFFSET_TABLE
            // For testing if offset table is the same
            var missingList = OffsetLocationsTemp.Except(OffsetLocations);
#endif

            for (int loc = 0; loc < OffsetLocations.Count; ++loc)
            {
                writer.WriteUInt32(OffsetLocations[loc]);
            }
            OffsetLocations.Clear();

            // Go back and write size
            writer.Endianness = Endianness.Little;
            {
                // It looks like it always tries to align to 32-bit
                writer.Seek(0, SeekOrigin.End);
                while ((writer.Length - writer.GetOffsetOrigin()) % 0x10 != 0)
                {
                    writer.WriteByte(0x00);
                }

                writer.Seek(writer.GetOffsetOrigin() + 4, SeekOrigin.Begin);
                writer.WriteUInt32((uint)writer.Length - dataStart);
                writer.Seek(0, SeekOrigin.End);
            }
            writer.Endianness = endianPrev;

            writer.PopOffsetOrigin();
        }

        public void Add(BinaryObjectWriter writer)
        {
            OffsetLocations.Add((uint)(writer.Position - writer.GetOffsetOrigin()));
        }
    }
}
