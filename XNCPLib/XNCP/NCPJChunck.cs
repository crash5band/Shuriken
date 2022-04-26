using System;
using System.IO;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class NCPJChunck
    {
        public uint Signature { get; set; }
        public uint Field08 { get; set; }
        public uint Field0C { get; set; }
        public string ProjectName { get; set; }
        public uint DXLSignature { get; set; }
        public CSDNode Root { get; set; }
        public FontList Fonts { get; set; }

        public NCPJChunck()
        {
            Root = new CSDNode();
            Fonts = new FontList();
        }

        public void Read(BinaryObjectReader reader)
        {
            reader.PushOffsetOrigin();
            Endianness endianPrev = reader.Endianness;

            // Header is always little endian
            uint size;
            reader.Endianness = Endianness.Little;
            {
                Signature = reader.ReadUInt32();
                size = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            Field08 = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();

            uint rootNodeOffset = reader.ReadUInt32();

            uint projectNameOffset = reader.ReadUInt32();
            ProjectName = reader.ReadStringOffset(projectNameOffset);

            DXLSignature = reader.ReadUInt32();
            uint fontListOffset = reader.ReadUInt32();

            reader.Seek(reader.GetOffsetOrigin() + rootNodeOffset, SeekOrigin.Begin);
            Root.Read(reader);

            reader.Seek(reader.GetOffsetOrigin() + fontListOffset, SeekOrigin.Begin);
            Fonts.Read(reader);

            // TODO: can we verify the the position after the last root/font matches the size?

            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            writer.PushOffsetOrigin();
            Endianness endianPrev = writer.Endianness;

            // Header is always little endian
            writer.Endianness = Endianness.Little;
            {
                writer.WriteUInt32(Signature);

                // Skipped: size
                writer.Skip(4);
            }
            writer.Endianness = endianPrev;
            uint dataStart = (uint)writer.Position;

            writer.WriteUInt32(Field08);
            writer.WriteUInt32(Field0C);

            // RootNodeOffset is always 0x20?
            offsetChunk.Add(writer);
            uint rootNodeOffset = 0x20;
            writer.WriteUInt32(rootNodeOffset);

            // ProjectNameOffset is always 0x38?
            offsetChunk.Add(writer);
            uint projectNameOffset = 0x38;
            writer.WriteUInt32(projectNameOffset);
            writer.WriteStringOffset(projectNameOffset, ProjectName);

            // Align to 4 bytes if the ProjectName wasn't
            writer.Seek(0, SeekOrigin.End);
            writer.Align(4);

            // FontListOffset points to right after ProjectName
            uint fontListOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
            writer.Seek(writer.GetOffsetOrigin() + 0x18, SeekOrigin.Begin);
            writer.WriteUInt32(DXLSignature);
            offsetChunk.Add(writer);
            writer.WriteUInt32(fontListOffset);
            
            Root.Write_Step0(writer);
            Fonts.Write_Step0(writer);
            Root.Write_Step1(writer, offsetChunk);
            Fonts.Write_Step1(writer, offsetChunk);
            Root.Write_Step2(writer, offsetChunk);
            Fonts.Write_Step2(writer, offsetChunk);
            Root.Write_Step3(writer, offsetChunk);

            // Go back and write size
            writer.Endianness = Endianness.Little;
            {
                // It looks like it always tries to align to 32-bit
                writer.Seek(0, SeekOrigin.End);
                while ((writer.Length - writer.GetOffsetOrigin()) % 0x10 != 0)
                {
                    writer.WriteByte(0x00);
                }

                writer.Seek(writer.GetOffsetOrigin() + 4, SeekOrigin.Begin);
                writer.WriteUInt32((uint)writer.Length - dataStart);
                writer.Seek(0, SeekOrigin.End);
            }
            writer.Endianness = endianPrev;

            writer.PopOffsetOrigin();
        }
    }
}