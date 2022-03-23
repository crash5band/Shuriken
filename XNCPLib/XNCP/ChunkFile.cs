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

            reader.SeekL(reader.GetOffsetOrigin() + Info.NextChunkOffset, SeekOrigin.Begin);

            // check whether the next chunk is a NCPJChunk or XTextureListChunk.
            // signature check is always little endian.
            bool bigEndian = reader.Endianness == Endianness.Big;
            reader.Endianness = Endianness.Little;
            uint nextSignature = reader.ReadUInt32();

            if (bigEndian)
                reader.Endianness = Endianness.Big;

            reader.SeekL(reader.GetOffsetOrigin() + Info.NextChunkOffset, SeekOrigin.Begin);
            if (nextSignature != Utilities.Make4CCLE("NXTL"))
            {
                CsdmProject.Read(reader);
            }
            else
            {
                TextureList.Read(reader);
            }

            reader.SeekL(reader.GetOffsetOrigin() + Info.NextChunkOffset + Info.ChunkListSize, SeekOrigin.Begin);
            Offset.Read(reader);
            End.Read(reader);

            reader.PopOffsetOrigin();
        }
    }
}
