using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Amicitia.IO.Binary;

namespace XNCPLib.XNCP
{
    public class SubImage
    {
        public uint TextureIndex { get; set; }
        public Vector2 TopLeft { get; set; }
        public Vector2 BottomRight { get; set; }

        public SubImage()
        {
            TextureIndex = 0;
            TopLeft     = new Vector2(0.0f, 0.0f);
            BottomRight = new Vector2(1.0f, 1.0f);
        }

        public void Read(BinaryObjectReader reader)
        {
            TextureIndex = reader.ReadUInt32();
            TopLeft     = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            BottomRight = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(TextureIndex);

            writer.WriteSingle(TopLeft.X);
            writer.WriteSingle(TopLeft.Y);

            writer.WriteSingle(BottomRight.X);
            writer.WriteSingle(BottomRight.Y);
        }
    }
}
