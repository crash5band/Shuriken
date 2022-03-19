using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP
{
    public class InfoChunk
    {
        public ChunkHeader Header { get; set; }
        public uint ChunkCount { get; set; }
        public uint NextChunkOffset { get; set; }
        public uint ChunkListSize { get; set; }
        public uint OffsetChunkOffset { get; set; }
        public uint OffsetChunkSize { get; set; }
        public uint Field1C { get; set; }

        public InfoChunk()
        {
            Header = new ChunkHeader();
        }

        public void Read(BinaryObjectReader reader)
        {
            Header.Read(reader);

            ChunkCount = reader.ReadUInt32();
            NextChunkOffset = reader.ReadUInt32();
            ChunkListSize = reader.ReadUInt32();
            OffsetChunkOffset = reader.ReadUInt32();
            OffsetChunkSize = reader.ReadUInt32();
            Field1C = reader.ReadUInt32();
        }
    }
}
