using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP.Animation
{
    public class Keyframe
    {
        public uint Frame { get; set; }
        public float Value { get; set; }
        public uint Field08 { get; set; }
        public float Offset1 { get; set; }
        public float Offset2 { get; set; }
        public uint Field14 { get; set; }

        public void Read(BinaryObjectReader reader)
        {
            Frame = reader.ReadUInt32();
            Value = reader.ReadSingle();
            Field08 = reader.ReadUInt32();
            Offset1 = reader.ReadSingle();
            Offset2 = reader.ReadSingle();
            Field14 = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(Frame);
            writer.WriteSingle(Value);
            writer.WriteUInt32(Field08);
            writer.WriteSingle(Offset1);
            writer.WriteSingle(Offset2);
            writer.WriteUInt32(Field14);
        }
    }
}
