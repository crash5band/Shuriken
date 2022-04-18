using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP
{
    public class FAPCEmbeddedRes
    {
        public ChunkFile Content { get; set; }

        public FAPCEmbeddedRes()
        {
            Content = new ChunkFile();
        }

        public void Read(BinaryObjectReader reader)
        {
            uint contentSize = reader.ReadUInt32();
            uint contentStart = (uint)reader.Position;
            {
                Content.Read(reader);
            }
            Debug.Assert(reader.Position - contentStart == contentSize);
        }

        public void Write(BinaryObjectWriter writer)
        {
            // Skipped: size
            writer.Skip(4);

            uint contentStart = (uint)writer.Position;
            {
                Content.Write(writer);
            }
            uint contentEnd = (uint)writer.Position;

            // Go back and write size
            writer.Seek(contentStart - 4, SeekOrigin.Begin);
            writer.WriteUInt32(contentEnd - contentStart);
            writer.Seek(contentEnd, SeekOrigin.Begin);
        }
    }
}
