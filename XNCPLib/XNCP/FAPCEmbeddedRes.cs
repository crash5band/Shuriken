using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

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

        public void Read(EndianBinaryReader reader)
        {
            Size = reader.ReadUInt32();
            Content.Read(reader);
        }
    }
}
