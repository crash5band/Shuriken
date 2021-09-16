using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class UICast
    {
        public uint Field00 { get; set; }
        public uint Field04 { get; set; }
        public uint IsEnabled { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomLeft { get; set; }
        public Vector2 TopRight { get; set; }
        public Vector2 BottomRight { get; set; }
        public ushort Field2C { get; set; }
        public ushort Field2E { get; set; }
        public uint CastInfoOffset { get; set; }
        public ushort Field34 { get; set; }
        public ushort Field36 { get; set; }
        public uint Field38 { get; set; }
        public uint Field3C { get; set; }
        public uint CastMaterialInfoOffset { get; set; }
        public StringOffset FontCharactersOffset { get; set; }
        public StringOffset FontNameOffset { get; set; }
        public uint Field4C { get; set; }
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

        public UICast()
        {
            FontCharactersOffset = new StringOffset();
            FontNameOffset = new StringOffset();

            Offset = new Vector2(0.0f, 0.0f);

            CastInfoData = new CastInfo();
            CastMaterialData = new CastMaterialInfo();
        }

        public void Read(EndianBinaryReader reader)
        {
            Field00 = reader.ReadUInt32();
            Field04 = reader.ReadUInt32();
            IsEnabled = reader.ReadUInt32();

            TopLeft = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            BottomLeft = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            TopRight = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            BottomRight = new Vector2(reader.ReadSingle(), reader.ReadSingle());

            Field2C = reader.ReadUInt16();
            Field2E = reader.ReadUInt16();
            CastInfoOffset = reader.ReadUInt32();
            Field34 = reader.ReadUInt16();
            Field36 = reader.ReadUInt16();
            Field38 = reader.ReadUInt32();
            Field3C = reader.ReadUInt32();
            CastMaterialInfoOffset = reader.ReadUInt32();

            FontCharactersOffset.Read(reader);
            FontNameOffset.Read(reader);

            Field4C = reader.ReadUInt32();
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Field58 = reader.ReadUInt32();
            Field5C = reader.ReadUInt32();

            Offset = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Field68 = reader.ReadSingle();
            Field6C = reader.ReadSingle();
            Field70 = reader.ReadUInt32();

            long baseOffset = reader.PeekBaseOffset();

            if (CastInfoOffset != 0)
            {
                reader.SeekBegin(baseOffset + CastInfoOffset);
                CastInfoData.Read(reader);
            }

            if (CastMaterialInfoOffset != 0)
            {
                reader.SeekBegin(baseOffset + CastMaterialInfoOffset);
                CastMaterialData.Read(reader);
            }
        }
    }
}
