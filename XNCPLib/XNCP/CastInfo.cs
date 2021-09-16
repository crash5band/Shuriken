using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class CastInfo
    {
        public int Field00 { get; set; }
        public Vector2 Translation { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public float Field18 { get; set; }
        public uint Color { get; set; }
        public uint GradientTopLeft { get; set; }
        public uint GradientBottomLeft { get; set; }
        public uint GradientTopRight { get; set; }
        public uint GradientBottomRight { get; set; }
        public uint Field30 { get; set; }
        public uint Field34 { get; set; }
        public uint Field38 { get; set; }

        public CastInfo()
        {

        }

        public void Read(EndianBinaryReader reader)
        {
            Field00     = reader.ReadInt32();
            Translation = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Rotation    = reader.ReadSingle();
            Scale       = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Field18     = reader.ReadSingle();

            Color               = reader.ReadUInt32();
            GradientTopLeft     = reader.ReadUInt32();
            GradientBottomLeft  = reader.ReadUInt32();
            GradientTopRight    = reader.ReadUInt32();
            GradientBottomRight = reader.ReadUInt32();

            Field30 = reader.ReadUInt32();
            Field34 = reader.ReadUInt32();
            Field38 = reader.ReadUInt32();
        }
    }
}
