using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

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

        public void Read(EndianBinaryReader reader)
        {
            reader.PushBaseOffset(reader.Position);
            Info = new InfoChunk();
            Info.Read(reader);

            reader.SeekBegin(reader.PeekBaseOffset() + Info.NextChunkOffset);

            // check whether the next chunk is a NCPJChunk or XTextureListChunk
            bool bigEndian = reader.Endianness == Endianness.BigEndian;
            reader.Endianness = Endianness.LittleEndian;
            uint nextSignature = reader.ReadUInt32();

            if (bigEndian)
                reader.Endianness = Endianness.BigEndian;

            reader.SeekBegin(reader.PeekBaseOffset() + Info.NextChunkOffset);
            if (nextSignature != Utilities.Utilities.Make4CCLE("NXTL"))
            {
                CsdmProject.Read(reader);
            }
            else
            {
                TextureList.Read(reader);
            }

            reader.SeekBegin(reader.PeekBaseOffset() + Info.NextChunkOffset + Info.ChunkListSize);
            Offset.Read(reader);
            End.Read(reader);

            reader.PopBaseOffset();
        }
    }
}
