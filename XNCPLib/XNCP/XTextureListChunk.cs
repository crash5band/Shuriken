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
    public class XTextureListChunk
    {
        public ChunkHeader Header { get; set; }
        public uint ListOffset { get; set; }
        public uint Field0C { get; set; }
        public uint TextureCount { get; set; }
        public uint DataOffset { get; set; }
        public List<XTexture> Textures { get; set; }

        public XTextureListChunk()
        {
            Header = new ChunkHeader();
            Textures = new List<XTexture>();
        }

        public void Read(BinaryObjectReader reader)
        {
            reader.PushOffsetOrigin();
            Header.Read(reader);

            ListOffset = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();
            TextureCount = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();

            reader.Seek(reader.GetOffsetOrigin() + DataOffset, SeekOrigin.Begin);
            for (int i = 0; i < TextureCount; ++i)
            {
                XTexture texture = new XTexture();
                texture.Read(reader);

                Textures.Add(texture);
            }

            reader.PopOffsetOrigin();
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.PushOffsetOrigin();
            Header.Write(writer);

            writer.WriteUInt32(ListOffset);
            writer.WriteUInt32(Field0C);
            writer.WriteUInt32(TextureCount);
            writer.WriteUInt32(DataOffset);

            writer.Seek(writer.GetOffsetOrigin() + DataOffset, SeekOrigin.Begin);
            for (int i = 0; i < TextureCount; ++i)
            {
                Textures[i].Write(writer);
            }

            writer.PopOffsetOrigin();
        }
    }
}
