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
        private uint ProjectNameOffset { get; set; }
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

            ProjectNameOffset = reader.ReadUInt32();
            ProjectName = reader.ReadStringOffset(ProjectNameOffset);

            DXLSignature = reader.ReadUInt32();
            FontListOffset = reader.ReadUInt32();

            reader.Seek(reader.GetOffsetOrigin() + RootNodeOffset, SeekOrigin.Begin);
            Root.Read(reader);

            reader.Seek(reader.GetOffsetOrigin() + FontListOffset, SeekOrigin.Begin);
            Fonts.Read(reader);

            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.PushOffsetOrigin();
            Header.Write(writer);

            writer.WriteUInt32(Field08);
            writer.WriteUInt32(Field0C);
            writer.WriteUInt32(RootNodeOffset);

            writer.WriteUInt32(ProjectNameOffset);
            writer.WriteStringOffset(ProjectNameOffset, ProjectName);

            writer.WriteUInt32(DXLSignature);
            writer.WriteUInt32(FontListOffset);

            writer.Seek(writer.GetOffsetOrigin() + RootNodeOffset, SeekOrigin.Begin);
            Root.Write(writer); // TODO: finish

            writer.Seek(writer.GetOffsetOrigin() + FontListOffset, SeekOrigin.Begin);
            Fonts.Write(writer);

            writer.PopOffsetOrigin();
        }
    }
}