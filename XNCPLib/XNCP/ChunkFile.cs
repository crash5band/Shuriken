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
        public uint OffsetChunkOffset { get; set; }
        public uint OffsetChunkSize { get; set; }
        public uint Field1C { get; set; }
        public uint NextSignature { get; set; }
        public NCPJChunck CsdmProject { get; set; }
        public XTextureListChunk TextureList { get; set; }
        public OffsetChunk Offset { get; set; }
        public EndChunk End { get; set; }

        public ChunkFile()
        {
            CsdmProject = new NCPJChunck();
            TextureList = new XTextureListChunk();
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

            uint chunkCount;
            uint nextChunkOffset;
            uint chunkListSize;
            uint headerStart = (uint)reader.Position;
            {
                chunkCount = reader.ReadUInt32(); // TODO: multiple chunk count
                nextChunkOffset = reader.ReadUInt32();
                chunkListSize = reader.ReadUInt32();
                OffsetChunkOffset = reader.ReadUInt32();
                OffsetChunkSize = reader.ReadUInt32();
                Field1C = reader.ReadUInt32();
            }
            Debug.Assert(reader.Position - headerStart == headerSize);

            //----------------------------------------------------------------
            // NCPJChunk/XTextureListChunk
            //----------------------------------------------------------------
            reader.Seek(reader.GetOffsetOrigin() + nextChunkOffset, SeekOrigin.Begin);

            // check whether the next chunk is a NCPJChunk or XTextureListChunk.
            // signature check is always little endian.
            reader.Endianness = Endianness.Little;
            {
                NextSignature = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            reader.Seek(reader.GetOffsetOrigin() + nextChunkOffset, SeekOrigin.Begin);
            if (NextSignature != Utilities.Make4CCLE("NXTL"))
            {
                CsdmProject.Read(reader);
            }
            else
            {
                TextureList.Read(reader);
            }
            // TODO: can we verify chunkListSize matches the current largest offset?

            // TODO:
            reader.Seek(reader.GetOffsetOrigin() + nextChunkOffset + chunkListSize, SeekOrigin.Begin);
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

                writer.WriteUInt32(OffsetChunkOffset);
                writer.WriteUInt32(OffsetChunkSize);
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
            if (NextSignature != Utilities.Make4CCLE("NXTL"))
            {
                CsdmProject.Write(writer);
            }
            else
            {
                TextureList.Write(writer);
            }

            // It looks like it always tries to align ChunkListSize to 32-bit
            uint chunkListEnd = (uint)writer.Length;
            uint unalignedBytes = (chunkListEnd - headerInfoEnd) % 0x10;
            if (unalignedBytes != 0)
            {
                chunkListEnd += 0x10 - unalignedBytes;
            }

            // Go back and write ChunkListSize
            writer.Seek(headerInfoStart + 8, SeekOrigin.Begin);
            writer.WriteUInt32(chunkListEnd - headerInfoEnd);
            writer.Seek(chunkListEnd, SeekOrigin.Begin);

            // TODO:
            // We're still not writing some stuff here...
            Offset.Write(writer);
            End.Write(writer);

            writer.PopOffsetOrigin();
        }
    }
}
