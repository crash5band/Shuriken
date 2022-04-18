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

namespace XNCPLib.XNCP
{
    public class XTextureListChunk
    {
        public bool IsUsed { get; set; }
        public uint Signature { get; set; }
        public uint Field0C { get; set; }
        public List<XTexture> Textures { get; set; }

        public XTextureListChunk()
        {
            Textures = new List<XTexture>();
        }

        public void Read(BinaryObjectReader reader)
        {
            IsUsed = true;

            reader.PushOffsetOrigin();
            Endianness endianPrev = reader.Endianness;

            // Header is always little endian
            uint size;
            reader.Endianness = Endianness.Little;
            {
                Signature = reader.ReadUInt32();
                size = reader.ReadUInt32();
            }
            reader.Endianness = endianPrev;

            uint listOffset = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();
            uint textureCount = reader.ReadUInt32();
            uint dataOffset = reader.ReadUInt32();

            reader.Seek(reader.GetOffsetOrigin() + dataOffset, SeekOrigin.Begin);
            for (int i = 0; i < textureCount; ++i)
            {
                XTexture texture = new XTexture();
                texture.Read(reader);

                Textures.Add(texture);
            }
            // TODO: can we verify the the position after the last texture name matches the size?

            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer, ref List<uint> offsetList)
        {
            writer.PushOffsetOrigin();
            Endianness endianPrev = writer.Endianness;

            // Header is always little endian
            writer.Endianness = Endianness.Little;
            {
                writer.WriteUInt32(Signature);

                // Skipped: size
                writer.Skip(4);
            }
            writer.Endianness = endianPrev;

            // TODO: is this always just 0x10?
            writer.WriteUInt32(0x10);
            writer.WriteUInt32(Field0C);

            writer.WriteUInt32((uint)Textures.Count());

            // TODO: DataOffset is always just 0x18?
            uint dataOffsetPosition = (uint)(writer.Position - writer.GetOffsetOrigin());
            writer.WriteUInt32(0x18);

            for (int i = 0; i < Textures.Count(); ++i)
            {
                offsetList.Add((uint)(writer.Position - writer.GetOffsetOrigin()));
                Textures[i].Write(writer);
            }

            // Now add DataOffset to offset list
            offsetList.Add(dataOffsetPosition);

            // Align to 4 bytes if the last texture name wasn't
            writer.Seek(0, SeekOrigin.End);
            writer.Align(4);

            // Go back and write size
            writer.Endianness = Endianness.Little;
            {
                writer.Seek(writer.GetOffsetOrigin() + 4, SeekOrigin.Begin);
                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
                writer.Seek(0, SeekOrigin.End);
            }
            writer.Endianness = endianPrev;

            writer.PopOffsetOrigin();
        }
    }
}
