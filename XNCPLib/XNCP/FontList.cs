using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class FontID
    {
        public string Name { get; set; }
        public uint Index { get; set; }

        public FontID()
        {

        }

        public void Read(BinaryObjectReader reader)
        {
            uint nameOffset = reader.ReadUInt32();
            Name = reader.ReadStringOffset(nameOffset);
            Index = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer, uint nameOffset)
        {
            writer.WriteUInt32(nameOffset);
            writer.WriteStringOffset(nameOffset, Name);
            writer.WriteUInt32(Index);
        }
    }

    public class FontList
    {
        public List<Font> Fonts { get; set; }
        public List<FontID> FontIDTable { get; set; }
        private uint UnwrittenPosition { get; set; }

        public FontList()
        {
            Fonts = new List<Font>();
            FontIDTable = new List<FontID>();
        }

        public void Read(BinaryObjectReader reader)
        {
            uint fontCount = reader.ReadUInt32();
            FontIDTable.Capacity = (int)fontCount;

            uint fontTableOffset = reader.ReadUInt32();
            uint fontIDTableOffset = reader.ReadUInt32();

            for (int f = 0; f < fontCount; ++f)
            {
                reader.Seek(reader.GetOffsetOrigin() + fontTableOffset + (8 * f), SeekOrigin.Begin);

                Font font = new Font();
                font.Read(reader);
                Fonts.Add(font);
            }

            for (int i = 0; i < fontCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + fontIDTableOffset + (8 * i), SeekOrigin.Begin);
                
                FontID id = new FontID();
                id.Read(reader);
                FontIDTable.Add(id);
            }
        }

        public void Write(BinaryObjectWriter writer, uint fontDataOffset, uint characterMappingOffset, uint fontNamesOffset)
        {
            Debug.Assert(Fonts.Count == FontIDTable.Count);

            writer.WriteUInt32((uint)Fonts.Count);
            if (Fonts.Count == 0)
            {
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
                return;
            }

            writer.WriteUInt32(fontDataOffset);
            uint fontIDTableOffset = fontDataOffset + (uint)Fonts.Count * 0x8;
            writer.WriteUInt32(fontIDTableOffset);

            for (int f = 0; f < Fonts.Count; ++f)
            {
                writer.Seek(writer.GetOffsetOrigin() + fontDataOffset + (8 * f), SeekOrigin.Begin);
                Fonts[f].Write(writer, characterMappingOffset);

                // Get the next mapping offset
                characterMappingOffset += (uint)Fonts[f].CharacterMappings.Count * 0x8;
            }

            for (int i = 0; i < Fonts.Count; ++i)
            {
                writer.Seek(writer.GetOffsetOrigin() + fontIDTableOffset + (8 * i), SeekOrigin.Begin);
                FontIDTable[i].Write(writer, fontNamesOffset);

                // Get the next name offset
                int nameLength = FontIDTable[i].Name.Length + 1;
                int unalignedBytes = nameLength % 0x4;
                if (unalignedBytes != 0)
                {
                    nameLength += 0x4 - unalignedBytes;
                }
                fontNamesOffset += (uint)nameLength;
            }
        }

        public void Write_Step0(BinaryObjectWriter writer)
        {
            Debug.Assert(Fonts.Count == FontIDTable.Count);

            // Allocate memory for FontList data
            writer.Seek(0, SeekOrigin.End);
            UnwrittenPosition = (uint)writer.Position;
            Utilities.PadZeroBytes(writer, 0xC);
        }

        public void Write_Step1(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill FontList data
            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
            writer.WriteUInt32((uint)Fonts.Count);
            if (Fonts.Count == 0)
            {
                writer.WriteUInt32(0);
                writer.WriteUInt32(0);
                return;
            }

            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            writer.WriteUInt32((uint)(writer.Length + Fonts.Count * 0x8 - writer.GetOffsetOrigin()));

            // Allocate memory for Fonts and FontIDOffsets data
            writer.Seek(0, SeekOrigin.End);
            UnwrittenPosition = (uint)writer.Position;
            Utilities.PadZeroBytes(writer, Fonts.Count * 0x10);
        }

        public void Write_Step2(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            if (Fonts.Count == 0) return;

            // Fill Fonts data
            for (int f = 0; f < Fonts.Count; ++f)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                Fonts[f].Write_Step0(writer, offsetChunk);
                // Finished
            }

            // Fill FontIDs data
            for (int i = 0; i < Fonts.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                offsetChunk.Add(writer);
                uint nameOffset = (uint)(writer.Length - writer.GetOffsetOrigin());
                FontIDTable[i].Write(writer, nameOffset);

                // Align to 4 bytes if the name wasn't
                writer.Seek(0, SeekOrigin.End);
                writer.Align(4);
            }
        }
    }
}
