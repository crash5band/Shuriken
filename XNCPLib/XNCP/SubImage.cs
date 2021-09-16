using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using AmicitiaLibrary.IO;

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

        public void Read(EndianBinaryReader reader)
        {
            TextureIndex = reader.ReadUInt32();
            TopLeft     = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            BottomRight = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
    }
}
