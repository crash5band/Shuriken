using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class FontID
    {
        public StringOffset Name { get; set; }
        public uint Index { get; set; }

        public FontID()
        {
            Name = new StringOffset();
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

        public void Read(EndianBinaryReader reader)
        {
            FontCount = reader.ReadUInt32();
            FontIDTable.Capacity = (int)FontCount;

            FontTableOffset = reader.ReadUInt32();
            FontIDTableOffset = reader.ReadUInt32();

            long pos = reader.Position;
            for (int f = 0; f < FontCount; ++f)
            {
                reader.SeekBegin(reader.PeekBaseOffset() + FontTableOffset + (8 * f));

                Font font = new Font();
                font.Read(reader);
                Fonts.Add(font);
            }

            for (int i = 0; i < FontCount; ++i)
            {
                reader.SeekBegin(reader.PeekBaseOffset() + FontIDTableOffset + (8 * i));
                
                FontID id = new FontID();
                id.Name.Read(reader);
                id.Index = reader.ReadUInt32();
                FontIDTable.Add(id);
            }
        }
    }
}
