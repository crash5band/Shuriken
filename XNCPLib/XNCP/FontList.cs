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
        public uint NameOffset { get; set; }
        public uint Index { get; set; }

        public FontID()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            NameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(NameOffset);
            Index = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(NameOffset);
            writer.WriteStringOffset(NameOffset, Name);
            writer.WriteUInt32(Index);
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
                reader.Seek(reader.GetOffsetOrigin() + FontTableOffset + (8 * f), SeekOrigin.Begin);

                Font font = new Font();
                font.Read(reader);
                Fonts.Add(font);
            }

            for (int i = 0; i < FontCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + FontIDTableOffset + (8 * i), SeekOrigin.Begin);
                
                FontID id = new FontID();
                id.Read(reader);
                FontIDTable.Add(id);
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(FontCount);
            writer.WriteUInt32(FontTableOffset);
            writer.WriteUInt32(FontIDTableOffset);

            for (int f = 0; f < FontCount; ++f)
            {
                writer.Seek(writer.GetOffsetOrigin() + FontTableOffset + (8 * f), SeekOrigin.Begin);
                Fonts[f].Write(writer);
            }

            for (int i = 0; i < FontCount; ++i)
            {
                writer.Seek(writer.GetOffsetOrigin() + FontIDTableOffset + (8 * i), SeekOrigin.Begin);
                FontIDTable[i].Write(writer);
            }
        }
    }
}
