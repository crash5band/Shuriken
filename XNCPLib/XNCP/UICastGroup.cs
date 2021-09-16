using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmicitiaLibrary.IO;

namespace XNCPLib.XNCP
{
    public class UICastGroup
    {
        public uint CastCount { get; set; }
        public uint CastTableOffset { get; set; }
        public uint Field08 { get; set; }
        public uint CastHierarchyTreeOffset { get; set; }
        public List<uint> CastOffsets { get; set; }
        public List<UICast> Casts { get; set; }
        public List<CastHierarchyTreeNode> CastHierarchyTree { get; set; }

        public UICastGroup()
        {
            Casts = new List<UICast>();
            CastOffsets = new List<uint>();
            CastHierarchyTree = new List<CastHierarchyTreeNode>();
        }

        public void Read(EndianBinaryReader reader)
        {
            CastCount = reader.ReadUInt32();
            Casts.Capacity = (int)CastCount;
            CastOffsets.Capacity = (int)CastCount;

            CastTableOffset = reader.ReadUInt32();
            Field08 = reader.ReadUInt32();
            CastHierarchyTreeOffset = reader.ReadUInt32();

            long baseOffset = reader.PeekBaseOffset();
            reader.SeekBegin(baseOffset + CastTableOffset);

            for (int i = 0; i < CastCount; ++i)
            {
                CastOffsets.Add(reader.ReadUInt32());
            }

            for (int i = 0; i < CastCount; ++i)
            {
                reader.SeekBegin(baseOffset + CastOffsets[i]);

                UICast cast = new UICast();
                cast.Read(reader);

                Casts.Add(cast);
            }

            reader.SeekBegin(baseOffset + CastHierarchyTreeOffset);
            for (int i = 0; i < CastCount; ++i)
            {
                CastHierarchyTree.Add(new CastHierarchyTreeNode(reader.ReadInt32(), reader.ReadInt32()));
            }
        }
    }
}
