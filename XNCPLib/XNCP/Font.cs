using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class CharacterMapping
    {
        public char SourceCharacter { get; set; }
        public uint SubImageIndex { get; set; }
    }

    public class Font
    {
        public uint CharacterCount { get; set; }
        public uint CharacterMappingTableOffset { get; set; }
        public List<CharacterMapping> CharacterMappings { get; set; }

        public Font()
        {
            CharacterMappings = new List<CharacterMapping>();
        }

        public void Read(EndianBinaryReader reader)
        {
            CharacterCount = reader.ReadUInt32();
            CharacterMappingTableOffset = reader.ReadUInt32();
            
            CharacterMappings.Capacity = (int)CharacterCount;
            reader.SeekBegin(reader.PeekBaseOffset() + CharacterMappingTableOffset);

            for (int m = 0; m < CharacterCount; ++m)
            {
                CharacterMapping mapping = new CharacterMapping();
                char[] c = reader.ReadChars(4);

                mapping.SourceCharacter = c[reader.Endianness == Endianness.LittleEndian ? 0 : 3];
                mapping.SubImageIndex = reader.ReadUInt32();
                CharacterMappings.Add(mapping);
            }
        }
    }
}
