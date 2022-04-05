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
        public ChunkHeader Header { get; set; }
        public uint[] Padding { get; set; }

        public EndChunk()
        {
            Header = new ChunkHeader();
            Padding = new uint[] { 0, 0 };
        }

        public void Read(BinaryObjectReader reader)
        {
            Header.Read(reader);
            Padding[0] = reader.ReadUInt32();
            Padding[1] = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            Header.Write(writer);
            writer.WriteUInt32(Padding[0]);
            writer.WriteUInt32(Padding[1]);
        }
    }
}
