using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class CastGroup
    {
        public uint Field08 { get; set; }
        public List<Cast> Casts { get; set; }
        public List<CastHierarchyTreeNode> CastHierarchyTree { get; set; }
        private uint UnwrittenPosition { get; set; }

        public CastGroup()
        {
            Casts = new List<Cast>();
            CastHierarchyTree = new List<CastHierarchyTreeNode>();
        }

        public void Read(BinaryObjectReader reader)
        {
            uint CastCount = reader.ReadUInt32();
            Casts.Capacity = (int)CastCount;
            List<uint> CastOffsets = new List<uint>((int)CastCount);

            uint CastTableOffset = reader.ReadUInt32();
            Field08 = reader.ReadUInt32();
            uint CastHierarchyTreeOffset = reader.ReadUInt32();

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

        public void Write_Step0(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            UnwrittenPosition = (uint)writer.Position;

            writer.WriteUInt32((uint)Casts.Count);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0x8;

            // Allocate memory for CastOffsets data
            uint newUnwrittenPosition = (uint)writer.Length;
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, Casts.Count * 0x4);

            writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
            writer.WriteUInt32(Field08);
            offsetChunk.Add(writer);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));
            UnwrittenPosition += 0x8;

            // Fill CastHierarchy data
            writer.Seek(0, SeekOrigin.End);
            for (int i = 0; i < Casts.Count; ++i)
            {
                writer.WriteInt32(CastHierarchyTree[i].ChildIndex);
                writer.WriteInt32(CastHierarchyTree[i].NextIndex);
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step1(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            uint newUnwrittenPosition = (uint)writer.Length;

            // Fill CastOffsets data
            for (int i = 0; i < Casts.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x4;

                offsetChunk.Add(writer);
                writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

                // Allocate memory for Cast data
                writer.Seek(0, SeekOrigin.End);
                Utilities.PadZeroBytes(writer, FAPCFile.Type == NinjaType.SonicNext ? 0X50 : 0x74);
            }

            UnwrittenPosition = newUnwrittenPosition;
        }

        public void Write_Step2(BinaryObjectWriter writer, OffsetChunk offsetChunk)
        {
            // Fill Cast data
            for (int i = 0; i < Casts.Count; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += (uint)(FAPCFile.Type == NinjaType.SonicNext ? 0X50 : 0x74);

                Casts[i].Write_Step0(writer, offsetChunk);
                // Finished
            }
        }
    }
}
