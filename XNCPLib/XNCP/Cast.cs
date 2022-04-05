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
        public uint CastInfoOffset { get; set; }
        public uint Field34 { get; set; }
        public uint Field38 { get; set; }
        public uint Field3C { get; set; }
        public uint CastMaterialInfoOffset { get; set; }
        public string FontCharacters{ get; set; }
        public uint FontCharactersOffset { get; set; }
        public string FontName { get; set; }
        public uint FontNameOffset { get; set; }
        public uint Field4C { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Field58 { get; set; }
        public uint Field5C { get; set; }
        public Vector2 Offset { get; set; }
        public float Field68 { get; set; }
        public float Field6C { get; set; }
        public uint FontSpacingCorrection { get; set; }
        public CastInfo CastInfoData { get; set; }
        public CastMaterialInfo CastMaterialData { get; set; }

        public Cast()
        {
            Offset = new Vector2(0.0f, 0.0f);

            CastInfoData = new CastInfo();
            CastMaterialData = new CastMaterialInfo();
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
            CastInfoOffset = reader.ReadUInt32();
            Field34 = reader.ReadUInt32();
            Field38 = reader.ReadUInt32();
            Field3C = reader.ReadUInt32();
            CastMaterialInfoOffset = reader.ReadUInt32();

            FontCharactersOffset = reader.ReadUInt32();
            FontCharacters = reader.ReadStringOffset(FontCharactersOffset);

            FontNameOffset = reader.ReadUInt32();
            FontName = reader.ReadStringOffset(FontNameOffset);

            Field4C = reader.ReadUInt32();
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Field58 = reader.ReadUInt32();
            Field5C = reader.ReadUInt32();

            Offset = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Field68 = reader.ReadSingle();
            Field6C = reader.ReadSingle();
            FontSpacingCorrection = reader.ReadUInt32();

            long baseOffset = reader.GetOffsetOrigin();

            if (CastInfoOffset != 0)
            {
                reader.Seek(baseOffset + CastInfoOffset, SeekOrigin.Begin);
                CastInfoData.Read(reader);
            }

            if (CastMaterialInfoOffset != 0)
            {
                reader.Seek(baseOffset + CastMaterialInfoOffset, SeekOrigin.Begin);
                CastMaterialData.Read(reader);
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
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
            writer.WriteUInt32(CastInfoOffset);
            writer.WriteUInt32(Field34);
            writer.WriteUInt32(Field38);
            writer.WriteUInt32(Field3C);
            writer.WriteUInt32(CastMaterialInfoOffset);

            writer.WriteUInt32(FontCharactersOffset);
            writer.WriteStringOffset(FontCharactersOffset, FontCharacters);

            writer.WriteUInt32(FontNameOffset);
            writer.WriteStringOffset(FontNameOffset, FontName);

            writer.WriteUInt32(Field4C);
            writer.WriteUInt32(Width);
            writer.WriteUInt32(Height);
            writer.WriteUInt32(Field58);
            writer.WriteUInt32(Field5C);

            writer.WriteSingle(Offset.X);
            writer.WriteSingle(Offset.Y);
            writer.WriteSingle(Field68);
            writer.WriteSingle(Field6C);
            writer.WriteUInt32(FontSpacingCorrection);

            long baseOffset = writer.GetOffsetOrigin();

            if (CastInfoOffset != 0)
            {
                writer.Seek(baseOffset + CastInfoOffset, SeekOrigin.Begin);
                CastInfoData.Write(writer);
            }

            if (CastMaterialInfoOffset != 0)
            {
                writer.Seek(baseOffset + CastMaterialInfoOffset, SeekOrigin.Begin);
                CastMaterialData.Write(writer);
            }
        }
    }
}
