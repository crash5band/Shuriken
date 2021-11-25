﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

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

        public void Read(EndianBinaryReader reader)
        {
            reader.PushBaseOffset(reader.Position);
            Header.Read(reader);

            ListOffset = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();
            TextureCount = reader.ReadUInt32();
            DataOffset = reader.ReadUInt32();

            reader.SeekBegin(reader.PeekBaseOffset() + DataOffset);
            for (int i = 0; i < TextureCount; ++i)
            {
                XTexture texture = new XTexture();
                texture.Read(reader);

                Textures.Add(texture);
            }

            reader.PopBaseOffset();
        }
    }
}