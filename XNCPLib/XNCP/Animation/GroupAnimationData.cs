using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP.Animation
{
    public class GroupAnimationData
    {
        public uint CastCount { get; set; }
        public uint CastDataOffset { get; set; }
        public List<CastAnimationData> CastAnimationDataList { get; set; }
        private uint UnwrittenPosition { get; set; }

        public GroupAnimationData()
        {
            CastAnimationDataList = new List<CastAnimationData>();
        }

        public void Read(BinaryObjectReader reader)
        {
            CastCount = reader.ReadUInt32();
            CastDataOffset = reader.ReadUInt32();

            CastAnimationDataList.Capacity = (int)CastCount;

            for (int i = 0; i < CastCount; ++i)
            {
                reader.Seek(reader.GetOffsetOrigin() + CastDataOffset + (8 * i), SeekOrigin.Begin);

                CastAnimationData animationData = new CastAnimationData();
                animationData.Read(reader);

                CastAnimationDataList.Add(animationData);
            }
        }

        public void Write(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(CastCount);
            writer.WriteUInt32(CastDataOffset);

            for (int i = 0; i < CastCount; ++i)
            {
                writer.Seek(writer.GetOffsetOrigin() + CastDataOffset + (8 * i), SeekOrigin.Begin);

                CastAnimationDataList[i].Write(writer);
            }
        }

        public void Write_Step0(BinaryObjectWriter writer)
        {
            writer.WriteUInt32(CastCount);
            writer.WriteUInt32((uint)(writer.Length - writer.GetOffsetOrigin()));

            // Allocate memory for CastAnimationDataList
            UnwrittenPosition = (uint)writer.Length;
            writer.Seek(0, SeekOrigin.End);
            Utilities.PadZeroBytes(writer, (int)CastCount * 0x8);
        }

        public void Write_Step1(BinaryObjectWriter writer)
        {
            // Fill CastAnimationDataList
            for (int i = 0; i < CastCount; ++i)
            {
                writer.Seek(UnwrittenPosition, SeekOrigin.Begin);
                UnwrittenPosition += 0x8;

                CastAnimationDataList[i].Write_Step0(writer);
            }
        }

        public void Write_Step2(BinaryObjectWriter writer)
        {
            // Continue CastAnimationDataList steps
            for (int i = 0; i < CastCount; ++i)
            {
                CastAnimationDataList[i].Write_Step1(writer);
                // Finished
            }
        }
    }

    public class GroupAnimationData2
    {
        public uint DataOffset { get; set; }
        public bool IsUsed { get; set; }
        public CastAnimationData2List AnimationData2List { get; set; }

        public GroupAnimationData2()
        {
            AnimationData2List = new CastAnimationData2List();
        }
    }

    public class GroupAnimationData2List
    {
        public uint Field00 { get; set; }
        public uint DataOffset { get; set; }
        public bool IsUsed { get; set; }
        public List<GroupAnimationData2> GroupList { get; set; }

        public GroupAnimationData2List()
        {
            GroupList = new List<GroupAnimationData2>();
        }
    }
}
