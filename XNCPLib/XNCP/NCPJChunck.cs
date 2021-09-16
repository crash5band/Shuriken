using System;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class NCPJChunck
    {
        public ChunkHeader Header { get; set; }
        public uint Field08 { get; set; }
        public uint Field0C { get; set; }
        public uint RootNodeOffset { get; set; }
        public StringOffset ProjectName { get; set; }
        public uint DXLSignature { get; set; }
        public uint FontListOffset { get; set; }
        public CSDNode Root { get; set; }
        public FontList Fonts { get; set; }

        public NCPJChunck()
        {
            Header = new ChunkHeader();
            ProjectName = new StringOffset();
            Root = new CSDNode();
            Fonts = new FontList();
        }

        public void Read(EndianBinaryReader reader)
        {
            reader.PushBaseOffset(reader.Position);
            Header = new ChunkHeader();
            Header.Read(reader);

            Field08 = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();
            RootNodeOffset = reader.ReadUInt32();

            ProjectName.Read(reader);

            DXLSignature = reader.ReadUInt32();
            FontListOffset = reader.ReadUInt32();

            reader.SeekBegin(reader.PeekBaseOffset() + RootNodeOffset);
            Root.Read(reader);

            reader.SeekBegin(reader.PeekBaseOffset() + FontListOffset);
            Fonts.Read(reader);

            reader.PopBaseOffset();
        }
    }
}