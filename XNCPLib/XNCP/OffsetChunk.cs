﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class OffsetChunk
    {
        public ChunkHeader Header { get; set; }
        public uint OffsetLocationCount { get; set; }
        public uint Field0C { get; set; }
        public List<uint> OffsetLocations { get; set; }

        public OffsetChunk()
        {
            Header = new ChunkHeader();
            OffsetLocations = new List<uint>();
        }

        public void Read(EndianBinaryReader reader)
        {
            long start = reader.Position;
            Header.Read(reader);

            OffsetLocationCount = reader.ReadUInt32();
            Field0C = reader.ReadUInt32();

            for (int loc = 0; loc < OffsetLocationCount; ++loc)
            {
                OffsetLocations.Add(reader.ReadUInt32());
            }

            reader.SeekBegin(Header.EndPosition);
        }
    }
}