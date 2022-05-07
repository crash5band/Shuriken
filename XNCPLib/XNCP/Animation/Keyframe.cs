using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP.Animation
{
    public enum KeyframeType
    {
        Const = 0,
        Linear = 1,
        Hermite = 2
    }

    public class Keyframe
    {
        public uint Frame { get; set; }
        public float Value { get; set; }
        public KeyframeType Type { get; set; }
        public float InTangent { get; set; }
        public float OutTangent { get; set; }
        public uint Field14 { get; set; }

        public void Read(BinaryObjectReader reader)
        {
            Frame = reader.ReadUInt32();
            Value = reader.ReadSingle();
            Type = (KeyframeType)reader.ReadUInt32();
            InTangent = reader.ReadSingle();
            OutTangent = reader.ReadSingle();
            Field14 = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(Frame);
            writer.WriteSingle(Value);
            writer.WriteUInt32((uint)Type);
            writer.WriteSingle(InTangent);
            writer.WriteSingle(OutTangent);
            writer.WriteUInt32(Field14);
        }
    }
}
