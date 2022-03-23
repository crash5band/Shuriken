using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class FontID
    {
        public string Name { get; set; }
        public uint Index { get; set; }

        public FontID()
        {

        }
    }

    public class FontList
    {
        public uint FontCount { get; set; }
        public uint FontTableOffset { get; set; }
        public uint FontIDTableOffset { get; set; }
        public List<Font> Fonts { get; set; }
        public List<FontID> FontIDTable { get; set; }

        public FontList()
        {
            Fonts = new List<Font>();
            FontIDTable = new List<FontID>();
        }

        public void Read(BinaryObjectReader reader)
        {
            FontCount = reader.ReadUInt32();
            FontIDTable.Capacity = (int)FontCount;

            FontTableOffset = reader.ReadUInt32();
            FontIDTableOffset = reader.ReadUInt32();

            long pos = reader.Position;
            for (int f = 0; f < FontCount; ++f)
            {
                reader.SeekL(reader.GetOffsetOrigin() + FontTableOffset + (8 * f), SeekOrigin.Begin);

                Font font = new Font();
                font.Read(reader);
                Fonts.Add(font);
            }

            for (int i = 0; i < FontCount; ++i)
            {
                reader.SeekL(reader.GetOffsetOrigin() + FontIDTableOffset + (8 * i), SeekOrigin.Begin);
                
                FontID id = new FontID();
                
                uint nameOffset = reader.ReadUInt32();
                id.Name = reader.ReadStringOffset(nameOffset);
                id.Index = reader.ReadUInt32();
                FontIDTable.Add(id);
            }
        }
    }
}
