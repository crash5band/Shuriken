using System;
using System.IO;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class NCPJChunck
    {
        public ChunkHeader Header { get; set; }
        public uint Field08 { get; set; }
        public uint Field0C { get; set; }
        public uint RootNodeOffset { get; set; }
        public string ProjectName { get; set; }
        public uint DXLSignature { get; set; }
        public uint FontListOffset { get; set; }
        public CSDNode Root { get; set; }
        public FontList Fonts { get; set; }

        public NCPJChunck()
        {
            Header = new ChunkHeader();
            Root = new CSDNode();
            Fonts = new FontList();
        }

        public void Read(BinaryObjectReader reader)
        {
            reader.PushOffsetOrigin();
            Header = new ChunkHeader();
            Header.Read(reader);

            Field08 = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();
            RootNodeOffset = reader.ReadUInt32();

            uint projectNameOffset = reader.ReadUInt32();
            ProjectName = reader.ReadStringOffset(projectNameOffset);

            DXLSignature = reader.ReadUInt32();
            FontListOffset = reader.ReadUInt32();

            reader.SeekL(reader.GetOffsetOrigin() + RootNodeOffset, SeekOrigin.Begin);
            Root.Read(reader);

            reader.SeekL(reader.GetOffsetOrigin() + FontListOffset, SeekOrigin.Begin);
            Fonts.Read(reader);

            reader.PopOffsetOrigin();
        }
    }
}