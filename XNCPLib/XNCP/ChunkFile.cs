using System;
using System.IO;
using System.Collections.Generic;
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
        public InfoChunk Info { get; set; }
        public NCPJChunck CsdmProject { get; set; }
        public XTextureListChunk TextureList { get; set; }
        public OffsetChunk Offset { get; set; }
        public EndChunk End { get; set; }

        private uint NextSignature { get; set; }

        public ChunkFile()
        {
            Info = new InfoChunk();
            CsdmProject = new NCPJChunck();
            TextureList = new XTextureListChunk();
            Offset = new OffsetChunk();
            End = new EndChunk();
        }

        public void Read(BinaryObjectReader reader)
        {
            reader.PushOffsetOrigin();
            Info = new InfoChunk();
            Info.Read(reader);

            reader.Seek(reader.GetOffsetOrigin() + Info.NextChunkOffset, SeekOrigin.Begin);

            // check whether the next chunk is a NCPJChunk or XTextureListChunk.
            // signature check is always little endian.
            bool bigEndian = reader.Endianness == Endianness.Big;
            reader.Endianness = Endianness.Little;
            NextSignature = reader.ReadUInt32();

            if (bigEndian)
                reader.Endianness = Endianness.Big;

            reader.Seek(reader.GetOffsetOrigin() + Info.NextChunkOffset, SeekOrigin.Begin);
            if (NextSignature != Utilities.Make4CCLE("NXTL"))
            {
                CsdmProject.Read(reader);
            }
            else
            {
                TextureList.Read(reader);
            }

            reader.Seek(reader.GetOffsetOrigin() + Info.NextChunkOffset + Info.ChunkListSize, SeekOrigin.Begin);
            Offset.Read(reader);
            End.Read(reader);

            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.PushOffsetOrigin();
            Info.Write(writer);

            writer.Seek(writer.GetOffsetOrigin() + Info.NextChunkOffset, SeekOrigin.Begin);

            // signature is always little endian.
            bool bigEndian = writer.Endianness == Endianness.Big;
            writer.Endianness = Endianness.Little;
            writer.WriteUInt32(NextSignature);

            if (bigEndian)
            {
                writer.Endianness = Endianness.Big;
            }

            writer.Seek(writer.GetOffsetOrigin() + Info.NextChunkOffset, SeekOrigin.Begin);
            if (NextSignature != Utilities.Make4CCLE("NXTL"))
            {
                CsdmProject.Write(writer);
            }
            else
            {
                TextureList.Write(writer);
            }

            writer.Seek(writer.GetOffsetOrigin() + Info.NextChunkOffset + Info.ChunkListSize, SeekOrigin.Begin);
            // We're still not writing some stuff here...
            Offset.Write(writer);
            End.Write(writer);

            writer.PopOffsetOrigin();
        }
    }
}
