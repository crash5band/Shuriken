using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class ChunkFile
    {
        public uint Signature { get; set; }
        public uint Field1C { get; set; }
        public NCPJChunck CsdmProject { get; set; }
        public XTextureListChunk TextureList { get; set; }
        public OffsetChunk Offset { get; set; }
        public EndChunk End { get; set; }

        public ChunkFile()
        {
            Offset = new OffsetChunk();
            End = new EndChunk();
        }

        public void Read(BinaryObjectReader reader)
        {
            reader.PushOffsetOrigin();
            Endianness endianPrev = reader.Endianness;

            //----------------------------------------------------------------
            // Header
            //----------------------------------------------------------------
            // Header is always little endian
            uint headerSize;
            reader.Endianness = Endianness.Little;
            {
                Signature = reader.ReadUInt32();
                headerSize = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            uint headerStart = (uint)reader.Position;
            uint chunkCount = reader.ReadUInt32(); // TODO: multiple chunk count
            uint nextChunkOffset = reader.ReadUInt32();
            uint chunkListSize = reader.ReadUInt32();
            uint offsetChunkOffset = reader.ReadUInt32();
            uint offsetChunkSize = reader.ReadUInt32();
            Field1C = reader.ReadUInt32();
            Debug.Assert(reader.Position - headerStart == headerSize);

            //----------------------------------------------------------------
            // NCPJChunk/XTextureListChunk
            //----------------------------------------------------------------
            reader.Seek(reader.GetOffsetOrigin() + nextChunkOffset, SeekOrigin.Begin);

            // check whether the next chunk is a NCPJChunk or XTextureListChunk.
            // signature check is always little endian.
            uint nextSignature;
            reader.Endianness = Endianness.Little;
            {
                nextSignature = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            reader.Seek(reader.GetOffsetOrigin() + nextChunkOffset, SeekOrigin.Begin);
            if (nextSignature != Utilities.Make4CCLE("NXTL"))
            {
                CsdmProject = new NCPJChunck();
                CsdmProject.Read(reader);
            }
            else
            {
                TextureList = new XTextureListChunk();
                TextureList.Read(reader);
            }

            reader.Seek(reader.GetOffsetOrigin() + offsetChunkOffset, SeekOrigin.Begin);
            Offset.Read(reader);
            End.Read(reader);

            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.PushOffsetOrigin();
            Endianness endianPrev = writer.Endianness;

            //----------------------------------------------------------------
            // Header
            //----------------------------------------------------------------
            // Header is always little endian
            writer.Endianness = Endianness.Little;
            {
                writer.WriteUInt32(Signature);

                // Skipped: header size
                writer.Skip(4);
            }
            writer.Endianness = endianPrev;

            uint headerInfoStart = (uint)writer.Position;
            {
                writer.WriteUInt32(1); // TODO: multiple Chunk count

                // Skipped: NextChunkOffset
                writer.Skip(4);

                // Skipped: ChunkListSize
                writer.Skip(4);

                // Skipped: OffsetChunkOffset
                writer.Skip(4);

                // Skipped: OffsetChunkSize
                writer.Skip(4);

                writer.WriteUInt32(Field1C);
            }
            uint headerInfoEnd = (uint)writer.Position;

            // Go back and write header size
            writer.Endianness = Endianness.Little;
            {
                writer.Seek(headerInfoStart - 4, SeekOrigin.Begin);
                writer.WriteUInt32(headerInfoEnd - headerInfoStart);
                writer.Seek(headerInfoEnd, SeekOrigin.Begin);
            }
            writer.Endianness = endianPrev;

            // Go back and write NextChunkOffset
            writer.Seek(headerInfoStart + 4, SeekOrigin.Begin);
            writer.WriteUInt32(headerInfoEnd - (uint)writer.GetOffsetOrigin());
            writer.Seek(headerInfoEnd, SeekOrigin.Begin);

            //----------------------------------------------------------------
            // NCPJChunk/XTextureListChunk
            //----------------------------------------------------------------
            Debug.Assert((CsdmProject == null) ^ (TextureList == null));
            if (CsdmProject != null)
            {
                CsdmProject.Write(writer, Offset);
            }
            else
            {
                TextureList.Write(writer, Offset);
            }

            // Go back and write ChunkListSize, OffsetChunkOffset, OffsetChunkSize
            writer.Seek(headerInfoStart + 8, SeekOrigin.Begin);
            writer.WriteUInt32((uint)writer.Length - headerInfoEnd);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            writer.WriteUInt32((uint)Offset.OffsetLocations.Count * 0x4 + 0x10);
            writer.Seek(0, SeekOrigin.End);

            Offset.Write(writer);
            End.Write(writer);

            writer.PopOffsetOrigin();
        }
    }
}
