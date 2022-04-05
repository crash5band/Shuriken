using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP
{
    public class FAPCEmbeddedRes
    {
        public uint Size { get; set; }
        public ChunkFile Content { get; set; }

        public FAPCEmbeddedRes()
        {
            Content = new ChunkFile();
        }

        public void Read(BinaryObjectReader reader)
        {
            Size = reader.ReadUInt32();
            Content.Read(reader);
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(Size);
            Content.Write(writer);
        }
    }
}
