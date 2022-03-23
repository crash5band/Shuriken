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

        public void Read(BinaryObjectReader reader)
        {
            CharacterCount = reader.ReadUInt32();
            CharacterMappingTableOffset = reader.ReadUInt32();
            
            CharacterMappings.Capacity = (int)CharacterCount;
            reader.SeekL(reader.GetOffsetOrigin() + CharacterMappingTableOffset, SeekOrigin.Begin);

            for (int m = 0; m < CharacterCount; ++m)
            {
                CharacterMapping mapping = new CharacterMapping();
                char[] c = new char[4];
                for (int i = 0; i < 4; ++i)
                {
                    string s = reader.ReadString(StringBinaryFormat.FixedLength, 1);
                    c[i] = s.Length > 0 ? s[0] : '.';
                }

                mapping.SourceCharacter = c[reader.Endianness == Endianness.Little ? 0 : 3];
                mapping.SubImageIndex = reader.ReadUInt32();
                CharacterMappings.Add(mapping);
            }
        }
    }
}
