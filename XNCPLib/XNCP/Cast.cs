using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class Cast
    {
        public uint Field00 { get; set; }
        public uint Field04 { get; set; }
        public uint IsEnabled { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomLeft { get; set; }
        public Vector2 TopRight { get; set; }
        public Vector2 BottomRight { get; set; }
        public uint Field2C { get; set; }
        public uint Field34 { get; set; }
        public uint Field38 { get; set; }
        public uint SubImageCount { get; set; }
        public string FontCharacters{ get; set; }
        public string FontName { get; set; }
        public float FontSpacingAdjustment { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Field58 { get; set; }
        public uint Field5C { get; set; }
        public Vector2 Offset { get; set; }
        public float Field68 { get; set; }
        public float Field6C { get; set; }
        public uint Field70 { get; set; }
        public CastInfo CastInfoData { get; set; }
        public CastMaterialInfo CastMaterialData { get; set; }

        public Cast()
        {
            Offset = new Vector2(0.0f, 0.0f);
        }

        public void Read(BinaryObjectReader reader)
        {
            Field00 = reader.ReadUInt32();
            Field04 = reader.ReadUInt32();
            IsEnabled = reader.ReadUInt32();

            TopLeft = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            BottomLeft = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            TopRight = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            BottomRight = new Vector2(reader.ReadSingle(), reader.ReadSingle());

            Field2C = reader.ReadUInt32();
            uint CastInfoOffset = reader.ReadUInt32();
            Field34 = reader.ReadUInt32();
            Field38 = reader.ReadUInt32();
            SubImageCount = reader.ReadUInt32();
            uint CastMaterialInfoOffset = reader.ReadUInt32();

            uint FontCharactersOffset = reader.ReadUInt32();
            if (FontCharactersOffset != 0)
            {
                FontCharacters = reader.ReadStringOffset(FontCharactersOffset);
            }

            uint FontNameOffset = reader.ReadUInt32();
            if (FontNameOffset != 0)
            {
                FontName = reader.ReadStringOffset(FontNameOffset);
            }

            FontSpacingAdjustment = reader.ReadSingle();

            // SONIC THE HEDGEHOG XNCPs don't have these fields.
            if (FAPCFile.Type != NinjaType.SonicNext)
            {
                Width = reader.ReadUInt32();
                Height = reader.ReadUInt32();
                Field58 = reader.ReadUInt32();
                Field5C = reader.ReadUInt32();
                Offset = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                Field68 = reader.ReadSingle();
                Field6C = reader.ReadSingle();
                Field70 = reader.ReadUInt32();
            }

            long baseOffset = reader.GetOffsetOrigin();

            if (CastInfoOffset != 0)
            {
                reader.Seek(baseOffset + CastInfoOffset, SeekOrigin.Begin);
                CastInfoData = new CastInfo();
                CastInfoData.Read(reader);
            }

            if (CastMaterialInfoOffset != 0)
            {
                reader.Seek(baseOffset + CastMaterialInfoOffset, SeekOrigin.Begin);
                CastMaterialData = new CastMaterialInfo();
                CastMaterialData.Read(reader);
            }
        }

        public void Write_Step0(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            uint unwrittenPosition = (uint)writer.Position;

            writer.WriteUInt32(Field00);
            writer.WriteUInt32(Field04);
            writer.WriteUInt32(IsEnabled);

            writer.WriteSingle(TopLeft.X);
            writer.WriteSingle(TopLeft.Y);
            writer.WriteSingle(BottomLeft.X);
            writer.WriteSingle(BottomLeft.Y);

            writer.WriteSingle(TopRight.X);
            writer.WriteSingle(TopRight.Y);
            writer.WriteSingle(BottomRight.X);
            writer.WriteSingle(BottomRight.Y);

            writer.WriteUInt32(Field2C);

            // CastMaterialInfo goes before CastInfo...
            if (CastInfoData != null)
            {
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length + 0x80 - writer.GetOffsetOrigin()));
            }
            else
            {
                writer.WriteUInt32(0);
            }

            writer.WriteUInt32(Field34);
            writer.WriteUInt32(Field38);
            writer.WriteUInt32(SubImageCount);

            if (CastMaterialData != null)
            {
                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            }
            else
            {
                writer.WriteUInt32(0);
            }
            unwrittenPosition += 0x44;

            // Fill CastMaterialInfo and CastInfo
            writer.Seek(0, SeekOrigin.End);
            if (CastMaterialData != null)
            {
                CastMaterialData.Write(writer);
            }
            if (CastInfoData != null)
            {
                CastInfoData.Write(writer);
            }

            writer.Seek(unwrittenPosition, SeekOrigin.Begin);
            if (FontCharacters != null)
            {
                offsetChunk.Add(writer);
                uint fontCharactersOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                writer.WriteUInt32(fontCharactersOffset);
                writer.WriteStringOffset(fontCharactersOffset, FontCharacters);

                // Align to 4 bytes if the name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }
            else
            {
                writer.WriteUInt32(0);
            }
            unwrittenPosition += 0x4;

            writer.Seek(unwrittenPosition, SeekOrigin.Begin);
            if (FontName != null)
            {
                offsetChunk.Add(writer);
                uint fontNameOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                writer.WriteUInt32(fontNameOffset);
                writer.WriteStringOffset(fontNameOffset, FontName);

                // Align to 4 bytes if the name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }
            else
            {
                writer.WriteUInt32(0);
            }
            unwrittenPosition += 0x4;

            writer.Seek(unwrittenPosition, SeekOrigin.Begin);
            writer.WriteSingle(FontSpacingAdjustment);

            // SONIC THE HEDGEHOG XNCPs don't have these fields.
            if (FAPCFile.Type != NinjaType.SonicNext)
            {
                writer.WriteUInt32(Width);
                writer.WriteUInt32(Height);
                writer.WriteUInt32(Field58);
                writer.WriteUInt32(Field5C);

                writer.WriteSingle(Offset.X);
                writer.WriteSingle(Offset.Y);
                writer.WriteSingle(Field68);
                writer.WriteSingle(Field6C);
                writer.WriteUInt32(Field70);
            }
        }
    }
}
