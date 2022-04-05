using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;

namespace XNCPLib.XNCP
{
    public class CastGroup
    {
        public uint CastCount { get; set; }
        public uint CastTableOffset { get; set; }
        public uint Field08 { get; set; }
        public uint CastHierarchyTreeOffset { get; set; }
        public List<uint> CastOffsets { get; set; }
        public List<Cast> Casts { get; set; }
        public List<CastHierarchyTreeNode> CastHierarchyTree { get; set; }

        public CastGroup()
        {
            Casts = new List<Cast>();
            CastOffsets = new List<uint>();
            CastHierarchyTree = new List<CastHierarchyTreeNode>();
        }

        public void Read(BinaryObjectReader reader)
        {
            CastCount = reader.ReadUInt32();
            Casts.Capacity = (int)CastCount;
            CastOffsets.Capacity = (int)CastCount;

            CastTableOffset = reader.ReadUInt32();
            Field08 = reader.ReadUInt32();
            CastHierarchyTreeOffset = reader.ReadUInt32();

            long baseOffset = reader.GetOffsetOrigin();
            reader.Seek(baseOffset + CastTableOffset, SeekOrigin.Begin);

            for (int i = 0; i < CastCount; ++i)
            {
                CastOffsets.Add(reader.ReadUInt32());
            }

            for (int i = 0; i < CastCount; ++i)
            {
                reader.Seek(baseOffset + CastOffsets[i], SeekOrigin.Begin);

                Cast cast = new Cast();
                cast.Read(reader);

                Casts.Add(cast);
            }

            reader.Seek(baseOffset + CastHierarchyTreeOffset, SeekOrigin.Begin);
            for (int i = 0; i < CastCount; ++i)
            {
                CastHierarchyTree.Add(new CastHierarchyTreeNode(reader.ReadInt32(), reader.ReadInt32()));
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(CastCount);
            writer.WriteUInt32(CastTableOffset);
            writer.WriteUInt32(Field08);
            writer.WriteUInt32(CastHierarchyTreeOffset);

            long baseOffset = writer.GetOffsetOrigin();
            writer.Seek(baseOffset + CastTableOffset, SeekOrigin.Begin);

            for (int i = 0; i < CastCount; ++i)
            {
                writer.WriteUInt32(CastOffsets[i]);
            }

            for (int i = 0; i < CastCount; ++i)
            {
                writer.Seek(baseOffset + CastOffsets[i], SeekOrigin.Begin);
                Casts[i].Write(writer);
            }

            writer.Seek(baseOffset + CastHierarchyTreeOffset, SeekOrigin.Begin);
            for (int i = 0; i < CastCount; ++i)
            {
                writer.WriteInt32(CastHierarchyTree[i].ChildIndex);
                writer.WriteInt32(CastHierarchyTree[i].NextIndex);
            }
        }
    }
}
