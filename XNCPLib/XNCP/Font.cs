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

        public void Read(BinaryObjectReader reader)
        {
            char[] c = new char[4];
            for (int i = 0; i < 4; ++i)
            {
                string s = reader.ReadString(StringBinaryFormat.FixedLength, 1);
                c[i] = s.Length > 0 ? s[0] : '.';
            }

            SourceCharacter = c[reader.Endianness == Endianness.Little ? 0 : 3];
            SubImageIndex = reader.ReadUInt32();
        }

        public void Write(BinaryObjectWriter writer)
        {
            char[] toWrite = { '.', '\0', '\0', '\0' };
            if (writer.Endianness == Endianness.Little)
            {
                toWrite[0] = SourceCharacter;
            }
            else
            {
                toWrite[3] = SourceCharacter;
            }

            writer.WriteString(StringBinaryFormat.FixedLength, new string(toWrite), 4);
            writer.WriteUInt32(SubImageIndex);
        }
    }

    public class Font
    {
        public List<CharacterMapping> CharacterMappings { get; set; }

        public Font()
        {
            CharacterMappings = new List<CharacterMapping>();
        }

        public void Read(BinaryObjectReader reader)
        {
            uint characterCount = reader.ReadUInt32();
            uint characterMappingTableOffset = reader.ReadUInt32();
            
            CharacterMappings.Capacity = (int)characterCount;
            reader.Seek(reader.GetOffsetOrigin() + characterMappingTableOffset, SeekOrigin.Begin);

            for (int m = 0; m < characterCount; ++m)
            {
                CharacterMapping mapping = new CharacterMapping();
                mapping.Read(reader);
                CharacterMappings.Add(mapping);
            }
        }

        public void Write(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            writer.WriteUInt32((uint)CharacterMappings.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

            writer.Seek(0, SeekOrigin.End);
            for (int m = 0; m < CharacterMappings.Count; ++m)
            {
                CharacterMappings[m].Write(writer);
            }
        }
    }
}
